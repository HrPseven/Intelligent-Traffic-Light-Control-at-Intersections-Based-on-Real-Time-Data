import torch
import torch.nn as nn
import torch.optim as optim
import random
import numpy as np
from collections import deque
import socket
import os
import sys
import subprocess
import ctypes.wintypes
import matplotlib.pyplot as plt
import math




du_pending_experience = None
du_pre_experience = None
du_weight = None
du_action = 0
du_is_random = True
sent_duration = 0

# la_pending_experience = None
# la_pre_experience = None
# la_weight = None
# la_action = 0
# la_is_random = True
# sent_lane = 0

global_conn = None


def Socket_Server():
    global file_path

    HOST = '127.0.0.1'  # Localhost
    PORT = 65432        # Match this in Unity

    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind((HOST, PORT))
            s.listen()
            s.settimeout(60)  # Timeout after 60 seconds
            print("Python server listening...")

            unity_exe = os.path.join(os.path.dirname(__file__), "TrafficSim", "AI-TR traffic system simulation.exe")
            try:
                subprocess.Popen([unity_exe])

            except Exception as e:
                # Get the actual (current) Documents folder path
                docs_path = os.path.join(get_documents_folder(), "AI-TR traffic system simulation")
                file_path = os.path.join(docs_path, "shared_data.txt")
                os.makedirs(docs_path, exist_ok=True)

                with open(file_path, 'w') as file:
                    file.write("2")
                sys.exit()

            try:
                conn, addr = s.accept()
            except socket.timeout:
                print("⏱️ No connection within 1 minute. Exiting.")
                sys.exit()

            # conn, addr = s.accept()

            global global_conn
            global_conn = conn

            with conn:
                print('Connected by', addr)
                print()

                buffer = ""
                while True:
                    data = conn.recv(1024)
                    if not data:
                        break
                    buffer += data.decode()

                    while "\n" in buffer:
                        line, buffer = buffer.split("\n", 1)
                        line = line.strip().lstrip('\ufeff')  # Remove BOM if present
                        # print(f"📨 Received raw line: '{line}'")
                        if line.startswith("DW:"):
                            parts = line.split(",R:")
                            flat_weights_str = parts[0][3:]
                            reward = float(parts[1]) if len(parts) > 1 else None

                            # Check for NaN
                            if reward is not None and math.isnan(reward):
                                print("⚠️ NaN reward received. Closing connection.")
                                sys.exit()
                                # break  # or exit() or socket.close()

                            # obs = np.array([int(x) for x in flat_weights_str.split(";")], dtype=np.float32)
                            obs = [float(x) for x in flat_weights_str.split(";")]

                            if reward is not None:
                                duration_ai_selector(obs, reward)
                            else:
                                duration_ai_selector(obs)

                        # elif line.startswith("LW:"):
                        #     parts = line.split(",R:")
                        #     flat_weights_str = parts[0][3:]
                        #     reward = float(parts[1]) if len(parts) > 1 else None
                        #
                        #     # obs = np.array([int(x) for x in flat_weights_str.split(";")], dtype=np.float32)
                        #     obs = [float(x) for x in flat_weights_str.split(";")]
                        #
                        #     # obs = obs / 100.0  # Example: scale to [0,1] if needed
                        #
                        #     if reward is not None:
                        #         lane_ai_selector(obs, reward)
                        #     else:
                        #         lane_ai_selector(obs)
            conn.close()
            print("❌ Connection closed")
    except Exception as e:
        print(f"❗Server error: {e}")
        sys.exit()


def duration_ai_selector(weight, reward = None):
    global du_pending_experience
    global du_pre_experience
    global du_weight
    global du_is_random
    global du_action
    global sent_duration
    global global_conn

    # weight = state_translator(weight)

    if  reward != None:

        reward = round(reward, 2)

        # print("✅ du_Weight:", du_weight)

        # print(f"⏭️, {duration_agent.epsilon}")

        if du_is_random:
            print("Exploration 🎲")
        else:
            print("BEST 🧠")

        print(f"📤 Sent duration: {sent_duration} ({du_action} from Q-values)")

        print(f"🎁Reward: {reward}")

        print(f"🎬, {du_action}")

        du_pending_experience = {
            "state": du_pre_experience["state"],
            "action": du_pre_experience["action"],
            "reward": reward,
            "next_state": np.array(weight, dtype=np.float32),
            "done": True
        }

        duration_agent.store_and_train(du_pending_experience, 128)
        print(f"🛣️ {du_pending_experience}")
        print()
        du_pending_experience = None

        state_tensor = torch.FloatTensor(du_pre_experience["state"]).unsqueeze(0).to(device)  # shape: [1, input_dim]

        with torch.no_grad():
            q_values = duration_agent.model(state_tensor)

        print("Q-values:", q_values.cpu().numpy()[0])  # Print Q-values per action
        print()

        print("🧠 Memory length:", len(duration_agent.memory))
        print("+++++++++++++++++++++++++++++++++++++++++++++++++++")
        print()

        # Save duration_agent
        trained_data = os.path.join(folder, "duration_agent.pth")
        latest_checkpoint = trained_data
        torch.save({
            'model_state_dict': duration_agent.model.state_dict(),
            'optimizer_state_dict': duration_agent.optimizer.state_dict(),
            'target_model_state_dict': duration_agent.target_model.state_dict(),  # optional
            'epsilon': duration_agent.epsilon,
            'memory': list(duration_agent.memory),
            'loss_history': duration_agent.loss_history
        }, latest_checkpoint)


    du_weight = weight
    du_action, du_is_random  = duration_agent.act(weight)
    du_pre_experience = {
        "state": np.array(weight, dtype=np.float32),
        "action": du_action
    }

    sent_duration = 10 + (du_action // 2) * 5 + (du_action % 2) * 2;
    # sent_duration = 10
    response = f"D:{int(round(sent_duration))}\n"
    global_conn.sendall(response.encode())


def get_documents_folder():
    # Get the real "Documents" folder path on Windows
    CSIDL_PERSONAL = 5
    SHGFP_TYPE_CURRENT = 0
    buf = ctypes.create_unicode_buffer(ctypes.wintypes.MAX_PATH)
    ctypes.windll.shell32.SHGetFolderPathW(None, CSIDL_PERSONAL, None, SHGFP_TYPE_CURRENT, buf)
    return buf.value

# device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
# # device = torch.device("cpu")
# print(f"Training on: {device}")

# Q-Network Definition
class du_QNetwork(nn.Module):
    def __init__(self, state_size, action_size):
        super(du_QNetwork, self).__init__()
        self.fc1 = nn.Linear(state_size, 128)
        self.fc2 = nn.Linear(128, 128)
        self.fc3 = nn.Linear(128, action_size)

    def forward(self, x):
        x = torch.relu(self.fc1(x))
        x = torch.relu(self.fc2(x))
        return self.fc3(x)

class DQLAgent:
    def __init__(self, state_size, action_size):
        self.state_size = state_size
        self.action_size = action_size
        self.memory = deque(maxlen=10000)
        self.gamma = 0.95
        self.epsilon = 1.0
        self.epsilon_min = 0.00  # 0.05 learning mode
        self.epsilon_decay = 0.999  # 0.999 learning mode
        self.learning_rate = 0.001

        self.model = du_QNetwork(state_size, action_size).to(device)
        self.target_model = du_QNetwork(state_size, action_size).to(device)

        self.target_model.load_state_dict(self.model.state_dict())  # Sync at start
        self.target_model.eval()  # Target network is not trained directly

        self.train_step = 0
        self.update_target_every = 200  # update every 100 training steps

        self.optimizer = optim.Adam(self.model.parameters(), lr=self.learning_rate)
        self.loss_fn = nn.MSELoss()
        self.loss_history = []  # Add this to your agent class (once)


    def act(self, state):
        if np.random.rand() <= self.epsilon:
            is_random = True
            action = random.randrange(self.action_size)
            return action, is_random

        # state = torch.FloatTensor([state]).unsqueeze(0).to(device)  # [1, 1]
        state = torch.from_numpy(np.array(state, dtype=np.float32)).unsqueeze(0).to(device)  # [1, input_size]

        with torch.no_grad():
            is_random = False
            q_values = self.model(state)
            action = torch.argmax(q_values).item()

        return action, is_random


    def remember(self, state, action, reward, next_state, done):
        self.memory.append((state, action, reward, next_state, done))


    def store_and_train(self, experience, batch_size):
        self.remember(
            experience["state"],
            experience["action"],
            experience["reward"],
            experience["next_state"],
            experience["done"]  # or True if episode ends
        )

        if(len(self.memory) % 1 == 0):
            self.replay(batch_size)
            print(f"⭐: {self.epsilon}")

        if (len(self.memory) % 200 == 0):
            plt.figure(figsize=(10, 5))
            plt.plot(self.loss_history)
            plt.title("Training Loss Over Time")
            plt.xlabel("Training Steps")
            plt.ylabel("Loss")
            plt.grid(True)
            plt.show()


    def replay(self, batch_size):
        if len(self.memory) < batch_size:
            return

        minibatch = random.sample(self.memory, batch_size)

        # states = torch.FloatTensor([x[0] for x in minibatch]).unsqueeze(1).to(device)
        # actions = torch.LongTensor([x[1] for x in minibatch]).unsqueeze(1).to(device)
        # rewards = torch.FloatTensor([x[2] for x in minibatch]).to(device)
        # next_states = torch.FloatTensor([x[3] for x in minibatch]).unsqueeze(1).to(device)
        # dones = torch.FloatTensor([x[4] for x in minibatch]).to(device)

        # states = torch.FloatTensor(np.vstack([x[0] for x in minibatch])).to(device)
        # next_states = torch.FloatTensor(np.vstack([x[3] for x in minibatch])).to(device)
        # actions = torch.tensor([x[1] for x in minibatch], dtype=torch.long, device=device)
        # rewards = torch.FloatTensor([x[2] for x in minibatch]).to(device)
        # dones = torch.FloatTensor([x[4] for x in minibatch]).to(device)

        # states, actions, rewards, next_states, dones = prepare_batch(minibatch, device)
        #
        # # Current Q-values
        # current_q = self.model(states).gather(1, actions.unsqueeze(1)).squeeze(1)
        #
        #
        # # Next max Q-values
        # next_q_values = self.model(next_states).max(1)[0]
        # targets = rewards + (self.gamma * next_q_values * (1 - dones))
        #
        # loss = self.loss_fn(current_q, targets)


        # Prepare data
        states = np.array([x[0] for x in minibatch], dtype=np.float32)
        actions = torch.tensor([x[1] for x in minibatch], dtype=torch.long, device=device)
        rewards = torch.tensor([x[2] for x in minibatch], dtype=torch.float32, device=device)
        next_states = np.array([x[3] for x in minibatch], dtype=np.float32)
        dones = torch.tensor([x[4] for x in minibatch], dtype=torch.float32, device=device)

        states_tensor = torch.from_numpy(states).to(device)
        next_states_tensor = torch.from_numpy(next_states).to(device)

        # Predict Q-values for current states
        q_values = self.model(states_tensor)
        state_action_values = q_values.gather(1, actions.unsqueeze(1)).squeeze(1)

        # Predict Q-values for next states
        next_q_values = self.model(next_states_tensor).detach().max(1)[0]
        # expected_state_action_values = rewards + self.gamma * next_q_values * (1 - dones)

        with torch.no_grad():
            target = rewards + self.gamma * next_q_values * (1 - dones)

        # Compute loss
        loss = self.loss_fn(state_action_values, target)
        self.loss_history.append(loss.item())

        # Optimize the model
        self.optimizer.zero_grad()
        loss.backward()
        self.optimizer.step()

        self.train_step += 1
        if self.train_step % self.update_target_every == 0:
            self.target_model.load_state_dict(self.model.state_dict())

        # Decay epsilon
        if self.epsilon > self.epsilon_min:
            self.epsilon *= self.epsilon_decay


def prepare_batch(minibatch, device):
    states = [x[0] for x in minibatch]
    next_states = [x[3] for x in minibatch]

    # Convert to numpy arrays with proper dimensions
    states_np = np.array(states, dtype=np.float32)
    next_states_np = np.array(next_states, dtype=np.float32)

    # If state is 1D per sample, add feature dim for PyTorch (batch_size, 1)
    if states_np.ndim == 1:
        states_np = states_np[:, None]
        next_states_np = next_states_np[:, None]

    states_tensor = torch.FloatTensor(states_np).to(device)
    next_states_tensor = torch.FloatTensor(next_states_np).to(device)

    actions = torch.tensor([x[1] for x in minibatch], dtype=torch.long, device=device)  # (batch_size,)
    rewards = torch.FloatTensor([x[2] for x in minibatch]).to(device)                   # (batch_size,)
    dones = torch.FloatTensor([x[4] for x in minibatch]).to(device)                     # (batch_size,)

    return states_tensor, actions, rewards, next_states_tensor, dones


def state_translator(state):
    state_new = []

    for i, value in enumerate(state):

        # Stopped and Moving cars Weight
        if i == 0:
            value = max(0, min(value, 80))
            value = value / 10
            value = round(value * 2) / 2
            # print(value)
            value = int((value * 2) - 1)
            value = max(0, min(value, 15))
            value = format(value, '04b') # "0111"


        # Lane Density
        if i == 1:
            value = float(value)
            value = max(0.0, min(value, 1.0))

        # Overall Density
        if i == 2:
            value = float(value)
            value = max(0.0, min(value, 1.0))

        # print(value)
        if isinstance(value, float):
            state_new.append(value)
        else:
            state_new = [int(bit) for bit in value]

        # state_new.append(value)
    return state_new


duration_experiences = None
def duration_train_agents():

    # Define destination folder and full file path
    folder = os.path.join(get_documents_folder(), "AI-TR traffic system simulation")
    os.makedirs(folder, exist_ok=True)

    duration_agent = DQLAgent(state_size=7, action_size=13)  # 10 durations

    # Load duration_agent
    trained_data = os.path.join(folder, "duration_agent.pth")
    if os.path.exists(trained_data):
        du_checkpoint = torch.load(trained_data, weights_only=False, map_location=torch.device('cpu'))
        duration_agent.model.load_state_dict(du_checkpoint['model_state_dict'])
        duration_agent.optimizer.load_state_dict(du_checkpoint['optimizer_state_dict'])
        duration_agent.epsilon = du_checkpoint['epsilon']
        duration_agent.memory = deque(du_checkpoint['memory'], maxlen=100000)
        duration_agent.loss_history = du_checkpoint['loss_history']


        loaded_model = (du_checkpoint['model_state_dict'])
        print(f"✅  loaded_model length: {len(loaded_model)}")

        loaded_target_model = (du_checkpoint['target_model_state_dict'])  # optional
        print(f"✅  loaded_target_model length: {len(loaded_target_model)}")

        loaded_optimizer = (du_checkpoint['optimizer_state_dict'])
        print(f"✅  loaded_optimizer length: {len(loaded_optimizer)}")

        loaded_epsilon = du_checkpoint['epsilon']
        print(f"✅  loaded_epsilon : {loaded_epsilon}")

        loaded_memory = deque(du_checkpoint['memory'], maxlen=100000)
        print(f"✅  loaded_memory length: {len(loaded_memory)}")

        loaded_loss = du_checkpoint['loss_history']
        print(f"✅  loaded_loss length: {len(loaded_loss)}")

        print("Duration_agent checkpoint loaded, resuming training.")

    duration_agent.model.eval()

    for i, exp in enumerate(loaded_memory):
        print(i)
        # if 1653 <= i <= 1673:
        #     print(f"Deleted")
        #     pass
        # else:
        print(f"Raw: {exp}")
        print()
        # state, action, reward, next_state, done = exp
        #
        # duration_experiences = {
        #     "state": state,
        #     "action": action,
        #     "reward": reward,
        #     "next_state": next_state,
        #     "done": True
        # }
        # duration_agent.store_and_train(duration_experiences, 256)
        # print(f"🛣️ {duration_experiences}")
        # duration_experiences = None
        #
        # print("☘️ la_direct length:", len(duration_agent.memory))
        # print("-------------------------------------------------")
        # print()

    # Save lane_agent
    # trained_data = os.path.join(folder, "duration_agentNew.pth")
    # latest_checkpoint = trained_data
    # torch.save({
    #     'model_state_dict': duration_agent.model.state_dict(),
    #     'target_model_state_dict': duration_agent.target_model.state_dict(),  # optional
    #     'optimizer_state_dict': duration_agent.optimizer.state_dict(),
    #     'epsilon': duration_agent.epsilon,
    #     'memory': list(duration_agent.memory),
    #     'loss_history': duration_agent.loss_history
    # }, latest_checkpoint)


def main():
    global device
    global duration_agent
    global folder

    # device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    device = torch.device("cpu")
    print(f"Training on: {device}")
    # duration_train_agents()

    duration_agent = DQLAgent(state_size=6, action_size=13)  # 13 durations

    # Define destination folder and full file path
    folder = os.path.join(get_documents_folder(), "AI-TR traffic system simulation")
    os.makedirs(folder, exist_ok=True)

    # Load duration_agent
    trained_data = os.path.join(folder, "duration_agent.pth")
    if os.path.exists(trained_data):
        du_checkpoint = torch.load(trained_data, weights_only=False, map_location=torch.device('cpu'))
        duration_agent.model.load_state_dict(du_checkpoint['model_state_dict'])
        duration_agent.target_model.load_state_dict(du_checkpoint['target_model_state_dict'])  # optional
        duration_agent.optimizer.load_state_dict(du_checkpoint['optimizer_state_dict'])
        # duration_agent.epsilon = du_checkpoint['epsilon']
        duration_agent.epsilon = 0
        duration_agent.memory = deque(du_checkpoint['memory'], maxlen=100000)
        duration_agent.loss_history = du_checkpoint['loss_history']
        # print("Duration_agent checkpoint loaded, resuming training.")
        print(f"✅ Loaded du_memory length: {len(duration_agent.memory)}")

    unique_states = set()

    for transition in duration_agent.memory:  # Replace with your actual buffer object
        state = transition[0]  # Assuming (state, action, reward, next_state, done)
        state_tuple = tuple(np.round(state, 3))
        unique_states.add(state_tuple)
    print(f"✅ Unique states in replay buffer: {len(unique_states)}")

    Socket_Server()

if __name__ == "__main__":
    main()

