from customtkinter import *
import tkinter as tk
from PIL import Image, ImageTk, ImageFilter
import time
import json
import subprocess
import random
import sys, os, shutil
import ctypes.wintypes


TR_check = False
def check_status(mode, frame):
    global TR_check

    line = ""
    if os.path.exists(file_path3):
        with open(file_path3, "r") as f:
            line = f.read()
        os.remove(file_path3)
        print("File deleted.")

    if line == "":
        if mode == 0:
            if os.path.exists(file_path1):
                if not TR_check:
                    TR_unity(mode)
                else:
                    result(0)
                    root.deiconify()
            else:
                root.deiconify()
                return_to_menu(event, source=frame)
                error()
                return
        else:
            root.deiconify()
            return_to_menu(event, source=frame)

    elif line == "0" or line == "2":
        print("Exit mode")
        root.deiconify()
        return_to_menu(event, source=frame)
        # if mode == 0:
        error()
        return
    else:
        print("Restart mode")
        AI_unity(mode)
    line = ""


def TR_unity(mode):
    print(f"TR, mode: {mode}")
    if mode == 0:
        global TR_check
        settings_values.update({'Benchmarking': 3})
        with open(file_path0, 'w') as file:
            json.dump(settings_values, file)

        try:
            unity_exe = os.path.join(os.path.dirname(__file__), "TrafficSim", "AI-TR traffic system simulation.exe")
            if os.path.exists(unity_exe):
                process = subprocess.Popen([unity_exe])
                # pic_path = r"D:\0000h\Documents\AI-TR traffic system simulation\wait.png"
                # subprocess.run(["cmd", "/c", "start", "/WAIT", pic_path], shell=True)

                process.wait()
            else:
                print("Unity simulation not found!")

        except Exception as e:
            root.deiconify()
            error()
            return

        TR_check = True
        check_status(mode, frame_benchmark)
    else:
        try:
            unity_exe = os.path.join(os.path.dirname(__file__), "TrafficSim", "AI-TR traffic system simulation.exe")
            if os.path.exists(unity_exe):
                process = subprocess.Popen([unity_exe])
                process.wait()
            else:
                print("Unity simulation not found!")

        except Exception as e:
            root.deiconify()
            error()
            return

        line = ""
        if os.path.exists(file_path3):
            with open(file_path3, "r") as f:
                line = f.read()
            os.remove(file_path3)
            print("File deleted.")
        if line == "0" or line == "2":
            root.deiconify()
            error()
            return
        else:
            root.deiconify()
            return_to_menu(event, source=frame_ordinary)

def AI_unity(mode):
    print(f"AI, mode: {mode}")
    if os.path.exists(file_path0):
        with open(file_path0, "r") as f:
            settings = json.load(f)

        try:
            AI_Logic = os.path.join(os.path.dirname(__file__), "ai_logic.py")

            # if settings.get("AI_terminal") == 1:
                # subprocess.run(['cmd.exe', '/c', 'start', '/wait', 'cmd.exe', '/c', AI_Logic], shell=True, check=True)
            # else:

            if os.path.exists(AI_Logic):
                # pass
                import ai_logic
                ai_logic.main()

                # subprocess.run([sys.executable, AI_Logic], check=True)
            else:
                print("Unity simulation not found!")

        except Exception as e:
            root.deiconify()
            error()
            return

    if mode == 0:
        check_status(mode, frame_benchmark)
    else:
        check_status(mode, frame_ai)

def start_simulation(button, v, f):
    global TR_check
    TR_check = False

    number = random.randint(1000, 9999)

    settings_values.update({'Benchmarking': v, 'randomness_seed':number})
    with open(file_path0, 'w') as file:
        json.dump(settings_values, file)
    button.configure(text="   Starting ...  ")
    root.after(900, lambda: button.configure(text="      Start      "))

    root.after(750, lambda: root.withdraw())

    # root.after(1000, lambda: (return_to_menu(source=f)))
    # root.after(1200, lambda: result())
    if v == 0 or v == 1:
        root.after(750, lambda: AI_unity(v))
    else:
        root.after(750, lambda: TR_unity(v))


def error(event=None):
    if globals()['exit_confirmation_window_open']:
        return

    globals()['exit_confirmation_window_open'] = True

    root_width = root.winfo_width()
    root_height = root.winfo_height()
    root_x = root.winfo_x()
    root_y = root.winfo_y()

    window_width = int(root.winfo_screenwidth() / 6.4)
    window_height = int(root.winfo_screenheight() / 7.2)

    x = root_x + (root_width // 2) - (window_width // 2)
    y = root_y + (root_height // 2) - (window_height // 2)

    new_window = CTkToplevel(root)
    new_window.geometry(f"{window_width}x{window_height}+{x}+{y}")
    new_window.title("Sudden stop")
    new_window.resizable(False, False)

    for i in range(2, 7):
        if globals()[f'button{i}'].winfo_exists():
            globals()[f'button{i}'].configure(state="disabled")

    CTkLabel(new_window,
             text="Something went wrong.",
             font=("Calibri", 14, "bold")
             )\
        .pack(pady=20)

    def check_button_state(event=None):
        for i in range(2, 7):
            if globals()[f'button{i}'].winfo_exists():
                globals()[f'button{i}'].configure(state="normal")

        root.attributes("-disabled", False)
        new_window.destroy()


    globals()['button_yes'] = CTkButton(new_window,
                           text="OK",
                           font=("Montserrat", 12),
                           width=80,
                           command=check_button_state
                           )
    globals()['button_yes'].pack(pady=10)
    globals()['button_yes'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_yes'], focused=True))
    globals()['button_yes'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_yes'], focused=False))


    new_window.protocol("WM_DELETE_WINDOW", check_button_state)
    new_window.transient(root)  # Make the window modal
    new_window.grab_set()  # Force interaction on this window only
    new_window.wait_window()    # Block further code until the window is closed
    globals()['exit_confirmation_window_open'] = False


ai_processed = False
def ai_based_traffic_light():
    global frame_menu
    global ai_processed
    global grid_yes

    time.sleep(0.01)
    if not ai_processed:
        global frame_ai
        frame_ai = CTkFrame(root, corner_radius=25, fg_color="#e7e7e7")
        frame_ai.grid(row=0, column=1, ipady=100, ipadx=107, pady=140, padx=(0,50), sticky="nsew", rowspan=2)
        frame_ai.rowconfigure([x for x in range(8)], weight=1)
        frame_ai.columnconfigure([x for x in range(2)], weight=1)

        globals()['label_ai'] = CTkLabel(frame_ai, text="AI-based traffic light:", font=("Montserrat", 24, "bold"))
        globals()['label_ai'].grid(row=0, column=0, pady=(25, 90))

        globals()['label_ai1'] = CTkLabel(frame_ai, text="AI-based T/L:", font=("Montserrat", 20))
        globals()['label_ai1'].grid(row=1, column=0)

        button_ai = CTkButton(frame_ai, command=lambda v=1,f=frame_ai :start_simulation(button_ai, v, f), text="      Start      ", font=("Montserrat", 14))
        button_ai.grid(row=1, column=1, sticky="w")
        button_ai.bind("<FocusIn>", lambda event: update_button_border(button_ai, focused=True))
        button_ai.bind("<FocusOut>", lambda event: update_button_border(button_ai, focused=False))

        globals()['label_ai_about'] = CTkLabel(frame_ai, text="Note: There is no durational restriction and no results will be saved.", font=("Open Sans", 12), wraplength=(400), justify="left")
        globals()['label_ai_about'].grid(row=4, column=0, pady=(0,50), columnspan=2, padx=(0,50))

        globals()['button_ai_back'] = CTkButton(frame_ai, text="       Back      ", font=("Montserrat", 16), command=lambda : (return_to_menu(source=frame_ai)))
        globals()['button_ai_back'].grid(row=5, column=0, columnspan=2, pady=(60,0))
        globals()['button_ai_back'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_ai_back'], focused=True))
        globals()['button_ai_back'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_ai_back'], focused=False))

        ai_processed = True
        grid_yes = False

    if grid_yes:
        frame_ai.grid(row=0, column=1, ipady=100, ipadx=107, pady=140, padx=(0, 50), sticky="nsew", rowspan=2)
    grid_yes = True
    update_font_size(event, source="ai_based_traffic_light", frame=frame_ai)
    # toggle_fullscreen(source="ai_based_traffic_light", frame=frame_ai, trigger=False)

    # root.bind("<Configure>", lambda event: update_font_size(event, source="settings"))
    root.bind("<Return>", focus_buttons)
    # root.bind("<F11>", lambda event: toggle_fullscreen(event, "ai_based_traffic_light", frame=frame_ai, trigger=True))
    root.bind("<Escape>", lambda event: (return_to_menu(event, source=frame_ai)))
    frame_menu.grid_forget()

ordinary_processed = False
def ordinary_traffic_light():
    global frame_menu
    global ordinary_processed
    global grid_yes

    time.sleep(0.01)
    if not ordinary_processed:
        global frame_ordinary
        frame_ordinary = CTkFrame(root, corner_radius=25, fg_color="#e7e7e7")
        frame_ordinary.grid(row=0, column=1, ipady=100, ipadx=107, pady=140, padx=(0,50), sticky="nsew", rowspan=2)
        frame_ordinary.rowconfigure([x for x in range(8)], weight=1)
        frame_ordinary.columnconfigure([x for x in range(2)], weight=1)

        globals()['label_ordinary'] = CTkLabel(frame_ordinary, text="Ordinary traffic light:", font=("Montserrat", 24, "bold"))
        globals()['label_ordinary'].grid(row=0, column=0, pady=(25, 90))

        globals()['label_ordinary1'] = CTkLabel(frame_ordinary, text="Ordinary T/L:", font=("Montserrat", 20))
        globals()['label_ordinary1'].grid(row=1, column=0)

        button_ordinary = CTkButton(frame_ordinary, command=lambda v=2, f=frame_ordinary: start_simulation(button_ordinary, v, f), text="      Start      ", font=("Montserrat", 14))
        button_ordinary.grid(row=1, column=1, sticky="w")
        button_ordinary.bind("<FocusIn>", lambda event: update_button_border(button_ordinary, focused=True))
        button_ordinary.bind("<FocusOut>", lambda event: update_button_border(button_ordinary, focused=False))

        globals()['label_ordinary_about'] = CTkLabel(frame_ordinary, text="Note: There is no durational restriction and no results will be saved.", font=("Open Sans", 12), wraplength=(400), justify="left")
        globals()['label_ordinary_about'].grid(row=4, column=0, pady=(0,50), columnspan=2, padx=(0,50))

        globals()['button_ordinary_back'] = CTkButton(frame_ordinary, text="       Back      ", font=("Montserrat", 16), command=lambda : (return_to_menu(source=frame_ordinary)))
        globals()['button_ordinary_back'].grid(row=5, column=0, columnspan=2, pady=(60,0))
        globals()['button_ordinary_back'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_ordinary_back'], focused=True))
        globals()['button_ordinary_back'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_ordinary_back'], focused=False))

        ordinary_processed = True
        grid_yes = False

    if grid_yes:
        frame_ordinary.grid(row=0, column=1, ipady=100, ipadx=107, pady=140, padx=(0, 50), sticky="nsew", rowspan=2)
    grid_yes = True
    update_font_size(event, source="ordinary_traffic_light", frame=frame_ordinary)
    # toggle_fullscreen(source="ordinary_traffic_light", frame=frame_ordinary, trigger=False)

    # root.bind("<Configure>", lambda event: update_font_size(event, source="settings"))
    root.bind("<Return>", focus_buttons)
    # root.bind("<F11>", lambda event: toggle_fullscreen(event, "ordinary_traffic_light", frame=frame_ordinary, trigger=True))
    root.bind("<Escape>", lambda event: (return_to_menu(event, source=frame_ordinary)))
    frame_menu.grid_forget()


def return_to_benchmark_list(event=None, source=None):
    benchmark(2)
    # frame_pr.grid(row=0, column=1, pady=140, padx=(0, 100), sticky="nsew", rowspan=2)
    frame_about.grid(row=0, column=0, ipady=100, pady=140, padx=(50, 50), sticky="nsew", rowspan=2)
    root.bind("<Escape>", lambda event: (return_to_menu(event, source=frame_pr)))
    # update_font_size(event, source="benchmark", frame=frame_pr)
    root.after(50, lambda: source.grid_forget())


def result(details):
    global frame_menu

    frame_result = CTkFrame(root, corner_radius=25, fg_color="#e7e7e7")
    frame_result.grid(row=0, column=1, pady=140, padx=(0, 100), sticky="nsew", rowspan=2)
    frame_result.rowconfigure([x for x in range(10)], weight=1)
    frame_result.columnconfigure([x for x in range(10)], weight=1)

    columns = ["Average Wait Time\n (s)", "Emergency Average\n Wait Time (s)", "Average Empty\n Green Light (s)", "Average Queue Length"]

    # Create table headers
    for col_index, col_name in enumerate(columns):
        label = CTkLabel(frame_result, text=col_name, width=100, height=30)
        label.grid(row=0, column=1 + col_index, padx=5, pady=(70,0))

    button_back = CTkButton(frame_result, text="       Back      ", font=("Montserrat", 16), command=lambda: (return_to_benchmark_list(source=frame_result)))
    button_back.grid(row=8, column=2, columnspan=2, sticky="", padx=(0,80))
    button_back.bind("<FocusIn>", lambda event: update_button_border(button_back, focused=True))
    button_back.bind("<FocusOut>", lambda event: update_button_border(button_back, focused=False))
    # details -= 1
    def on_row_click(details_num):
        if os.path.exists(file_path2):
            # Open and load the file
            with open(file_path2, 'r') as file:
                new_data = json.load(file)

            if details_num == 0:
                details_num = latest_row_number = len(new_data)

            AI_AverageQueueLength = 0
            AI_AverageWaitTime = 0
            AI_EmergencyAverageWaitTime = 0
            AI_AverageEmptyGreenLight = 0

            TR_AverageQueueLength = 0
            TR_AverageWaitTime = 0
            TR_EmergencyAverageWaitTime = 0
            TR_AverageEmptyGreenLight = 0

            for index, (datetime_key, data) in enumerate(new_data.items()):
                if details_num == index + 1:

                    Data_label = CTkLabel(frame_result, text=datetime_key, width=125, height=30, corner_radius=5)
                    Data_label.grid(row=0, column=0, pady=(70,0), padx=(20, 5))

                    if "AI" in data:
                        AI_label = CTkLabel(frame_result, text="AI", width=125, height=30, corner_radius=5)
                        AI_label.grid(row=1, column=0, pady=(0, 15), padx=(20, 5))

                        for index, (k, v) in enumerate(data["AI"].items()):

                            if index == 0 and v != "nan": AI_AverageWaitTime  = float(v)
                            elif index == 1 and v != "nan": AI_EmergencyAverageWaitTime  = float(v)
                            elif index == 2 and v != "nan": AI_AverageEmptyGreenLight  = float(v)
                            elif index == 3 and v != "nan": AI_AverageQueueLength = float(v)

                            AI_value_label = CTkLabel(frame_result, text=str(v), width=125, height=30,
                                                   fg_color="white", corner_radius=5)
                            AI_value_label.grid(row=1, column=1 + index, pady=(0, 15), padx=(0, 5))

                    if "TR" in data:
                        TR_label = CTkLabel(frame_result, text="TR", width=125, height=30, corner_radius=5)
                        TR_label.grid(row=2, column=0, pady=(0, 15), padx=(20, 5))

                        for index, (k, v) in enumerate(data["TR"].items()):

                            if index == 0 and v != "nan": TR_AverageWaitTime = float(v)
                            elif index == 1 and v != "nan": TR_EmergencyAverageWaitTime  = float(v)
                            elif index == 2 and v != "nan": TR_AverageEmptyGreenLight  = float(v)
                            elif index == 3 and v != "nan": TR_AverageQueueLength = float(v)

                            TR_value_label = CTkLabel(frame_result, text=str(v), width=125, height=30,
                                                   fg_color="white", corner_radius=5)
                            TR_value_label.grid(row=2, column=1 + index, pady=(0, 15), padx=(0, 5))

                    percentage_label = CTkLabel(frame_result, text="AI Impact vs Traditional\n(% Difference)", width=125, height=30, corner_radius=5)
                    percentage_label.grid(row=3, column=0, pady=(0, 15), padx=(20, 5))

                    def improvement_label(improvement, column):
                        # print(improvement)
                        if improvement >= 0:
                            improvement_label = CTkLabel(frame_result, text=f"{round(improvement, 2)} %", width=125, height=30,
                                                      fg_color="white", corner_radius=5, text_color="green", font=("Montserrat", 14, "bold"))
                        else:
                            improvement_label = CTkLabel(frame_result, text=f"{round(improvement, 2)} %", width=125, height=30,
                                                      fg_color="white", corner_radius=5, text_color="red", font=("Montserrat", 14, "bold"))

                        improvement_label.grid(row=3, column=column, pady=(0, 15), padx=(0, 5))

                    if isinstance(AI_AverageWaitTime, float) and isinstance(TR_AverageWaitTime, float):
                        improvement = ((TR_AverageWaitTime - AI_AverageWaitTime) / AI_AverageWaitTime) * 100
                        improvement_label(improvement, 1)

                    if isinstance(AI_EmergencyAverageWaitTime, float) and isinstance(TR_EmergencyAverageWaitTime, float):
                        improvement = ((TR_EmergencyAverageWaitTime - AI_EmergencyAverageWaitTime) / AI_EmergencyAverageWaitTime) * 100
                        improvement_label(improvement, 2)

                    if isinstance(AI_AverageEmptyGreenLight, float) and isinstance(TR_AverageEmptyGreenLight, float):

                        if AI_AverageEmptyGreenLight == 0:
                            improvement = TR_AverageEmptyGreenLight * 100 # or 0, or some custom value or message
                        else:
                            improvement = ((TR_AverageEmptyGreenLight - AI_AverageEmptyGreenLight) / AI_AverageEmptyGreenLight) * 100
                        improvement_label(improvement, 3)

                    # if isinstance(AI_AverageQueueLength, float) and isinstance(TR_AverageQueueLength, float):
                    #     improvement = ((AI_AverageQueueLength - TR_AverageQueueLength) / AI_AverageQueueLength) * 100
                    #     improvement_label(improvement, 4)
                    #
                    if isinstance(AI_AverageQueueLength, float) and isinstance(TR_AverageQueueLength, float):
                        improvement = ((TR_AverageQueueLength - AI_AverageQueueLength) / AI_AverageQueueLength) * 100
                        improvement_label(improvement, 4)


    # Check if the file exists
    if details == 0:
        if os.path.exists(file_path1):
            with open(file_path1, 'r') as file:
                new_data = json.load(file)

            categorized_results = {}
            data_value = new_data.get("Date", "Date not found")
            splitted_data = "\n".join(data_value.split(" ", 1))
            del new_data["Date"]

            # density_value = {"Very Low":0, "Low":1, "Normal":2, "High":3, "Very High":4, "Max":5}
            speed_value = {"Normal":1.0 , "Fast":2.0}
            duration_value = {"16 mins": 960, "32 mins": 1920, "2h": 7200, "32 mins": 1920, "64 mins": 3840}

            # for keys, values in density_value.items():
            #     if new_data["Density"] == values:
            #         new_data.update({"Density": f"{keys}"})

            for keys, values in speed_value.items():
                if new_data["Overall_speed"] == values:
                    new_data.update({"Overall_speed": f"{keys}"})

            for keys, values in duration_value.items():
                if new_data["Duration"] == values:
                    new_data.update({"Duration": f"{keys}"})

            if new_data["Pedestrians"] == 1:
                new_data.update({"Pedestrians": "Yes"})
            else:
                new_data.update({"Pedestrians": "No"})

            if os.path.exists(file_path2):
                with open(file_path2, 'r') as file:
                    results = json.load(file)
                results[f"{splitted_data}"] = new_data

                # Write the updated data back to the JSON file
                with open(file_path2, 'w') as file:
                    json.dump(results, file, indent=4)
            else:
                categorized_results[f"{splitted_data}"] = new_data
                with open(file_path2, 'w') as file:
                    json.dump(categorized_results, file, indent=4)
            print("Saved")
            on_row_click(0)
    else:
        on_row_click(details)


    update_font_size(event, source="result", frame=frame_result)
    # toggle_fullscreen(source="result", frame=frame_result, trigger=False)
    root.bind("<Return>", focus_buttons)
    # root.bind("<F11>", lambda event: toggle_fullscreen(event, "result", frame=frame_result, trigger=True))
    root.bind("<Escape>", lambda event: (return_to_benchmark_list(event, source=frame_result)))
    if 'frame_pr' in globals():
        frame_pr.grid_forget()
    frame_about.grid_forget()


benchmark_processed = False
def benchmark(mode = None):
    global frame_menu
    global benchmark_processed
    global grid_yes
    global results_processed
    global previous_results

    if mode != None and mode == 2:
        previous_results()
        return

    time.sleep(0.01)
    if not benchmark_processed:
        global frame_benchmark
        frame_benchmark = CTkFrame(root, corner_radius=25, fg_color="#e7e7e7")
        frame_benchmark.grid(row=0, column=1, ipady=100, ipadx=107, pady=140, padx=(0,50), sticky="nsew", rowspan=2)
        frame_benchmark.rowconfigure([x for x in range(8)], weight=1)
        frame_benchmark.columnconfigure([x for x in range(2)], weight=1)

        def return_to_benchmark(event=None, source=None):
            frame_benchmark.grid(row=0, column=1, ipady=100, ipadx=107, pady=140, padx=(0, 50), sticky="nsew", rowspan=2)
            root.bind("<Escape>", lambda event: (return_to_menu(event, source=frame_benchmark)))
            update_font_size(event, source="benchmark", frame=frame_benchmark)
            # root.bind("<F11>", lambda event: toggle_fullscreen(event, "benchmark", frame=frame_benchmark, trigger=True))
            root.after(50, lambda: source.grid_forget())

        results_processed = False
        def previous_results():
            global results_processed
            global grid_yes1
            time.sleep(0.01)
            if not results_processed:
                global frame_pr
                frame_pr = CTkFrame(root, corner_radius=25, fg_color="#e7e7e7")
                frame_pr.grid(row=0, column=1, pady=140, padx=(0, 100), sticky="nsew", rowspan=2)
                frame_pr.rowconfigure([x for x in range(8)], weight=1)
                frame_pr.columnconfigure([x for x in range(8)], weight=1)

                # Scrollable frame
                scroll_frame = CTkScrollableFrame(frame_pr, width=400, height=300)
                scroll_frame.grid(row=1, column=0, columnspan=6 , sticky="nsew", padx=(15, 15))

                def delete_results():
                    for checkbox in check_keys:
                        checkbox_value = checkbox.get()
                        if checkbox_value:
                            for keys, values in checkbox_list.items():
                                if keys == checkbox:
                                    del new_data[f"{values}"]
                                    with open(file_path2, 'w') as file:
                                        json.dump(new_data, file, indent=4)
                            frame_pr.destroy()
                            previous_results()

                def all():
                    check = check_all.get()
                    if check:
                        for checkbox in checkbox_list:
                            checkbox.select()
                    else:
                        for checkbox in checkbox_list:
                            checkbox.deselect()

                columns = ["                    Date", "            Overall Speed", "Duration", "Pedestrians"]

                # Create table headers
                for col_index, col_name in enumerate(columns):
                    label = CTkLabel(frame_pr, text=col_name, width=180, height=30)
                    label.grid(row=0, column=1+col_index, pady=10, padx= 2)

                make_about_frame_hidden = CTkLabel(frame_pr, text="", width=180, height=30)
                make_about_frame_hidden.grid(row=0, column=5, pady=10, padx= 2)

                check_all = CTkCheckBox(frame_pr, text="All", command=all)
                check_all.grid(row=0, column=0, sticky="w", padx=(20, 0))

                button_back = CTkButton(frame_pr, text="       Back      ", font=("Montserrat", 16), command=lambda: (return_to_benchmark(source=frame_pr)))
                button_back.grid(row=3, column=2, sticky="w")
                button_back.bind("<FocusIn>", lambda event: update_button_border(button_back, focused=True))
                button_back.bind("<FocusOut>", lambda event: update_button_border(button_back, focused=False))

                button_delete = CTkButton(frame_pr, text="      Delete      ", font=("Montserrat", 16), command=delete_results)
                button_delete.grid(row=3, column=4)
                button_delete.bind("<FocusIn>", lambda event: update_button_border(button_delete, focused=True))
                button_delete.bind("<FocusOut>", lambda event: update_button_border(button_delete, focused=False))


                # Check if the file exists
                if os.path.exists(file_path2):
                    # Open and load the file
                    with open(file_path2, 'r') as file:
                        new_data = json.load(file)

                    def on_hover_enter(event):
                        event.widget.configure( cursor="hand2")  # light blue and hand cursor

                    def on_hover_leave(event):
                        event.widget.configure( cursor="arrow")  # reset

                    check_keys = []
                    checkbox_list = {}
                    index_sec = 1


                    row_frame = CTkFrame(scroll_frame, fg_color="transparent")

                    for col_index, keys in enumerate(new_data.keys()):
                        value_label = CTkLabel(row_frame, text=str(keys), width=125, height=30, fg_color="white",
                                               corner_radius=5)
                        value_label.grid(row=1 + col_index, column=1, sticky="w", pady=(0, 15), padx=(0, 5))

                        value_label.bind("<Enter>", on_hover_enter)
                        value_label.bind("<Leave>", on_hover_leave)
                        value_label.bind("<Button-1>", lambda e, f=index_sec: result(f))

                        check_results = CTkCheckBox(row_frame, text="")
                        check_results.grid(row=1 + col_index, column=0, sticky="nsew", pady=(0, 15))
                        checkbox_list[check_results] = keys
                        check_keys.append(check_results)
                        index_sec += 1

                    index = 1
                    for key, value in new_data.items():
                        new_values = value

                        row_frame.grid(row=index, column=2, columnspan=10, sticky="w", padx=(0, 5))

                        for col_index, (k, val) in enumerate(new_values.items()):
                            value_label = CTkLabel(row_frame, text=str(val), width=125, height=30,
                                                   fg_color="white", corner_radius=5)
                            value_label.grid(row=index, column=2 + col_index, pady=(0, 15), padx=(0, 5))

                            # Make labels clickable too (they block frame click otherwise)
                            value_label.bind("<Enter>", on_hover_enter)
                            value_label.bind("<Leave>", on_hover_leave)
                            value_label.bind("<Button-1>", lambda e, f=index: result(f))
                            if k == "Pedestrians":
                                break
                        index += 1

                else:
                    globals()['No_date'] = CTkLabel(scroll_frame, text="No record has been saved.", font=("Montserrat", 20))
                    globals()['No_date'].grid(row=1, column=0, padx=225, pady=125)

                    button_delete.configure(state="disabled")
                    check_all.configure(state="disabled")

                # results_processed = True
                # grid_yes1 = False

            # if grid_yes1:
            #     frame_pr.grid(row=0, column=1, pady=140, padx=(0, 50), sticky="nsew", rowspan=2)
            # grid_yes1 = True
            update_font_size(event, source="view_results", frame=frame_pr)
            # toggle_fullscreen(source="view_results", frame=frame_pr, trigger=False)
            root.bind("<Return>", focus_buttons)
            # root.bind("<F11>", lambda event: toggle_fullscreen(event, "view_results", frame=frame_pr, trigger=True))
            root.bind("<Escape>", lambda event: (return_to_benchmark(event, source=frame_pr)))
            frame_benchmark.grid_forget()
            # frame_about.grid_forget()

        globals()['label_benchmark'] = CTkLabel(frame_benchmark, text="Benchmark:",font=("Montserrat", 30, "bold"))
        globals()['label_benchmark'].grid(row=0, column=0, pady=(25, 30))

        globals()['label_benchmark_ai'] = CTkLabel(frame_benchmark, text="Benchmarking:", font=("Montserrat", 20))
        globals()['label_benchmark_ai'].grid(row=1, column=0)

        button_benchmark_ai = CTkButton(frame_benchmark, command= lambda v=0, f=frame_benchmark:start_simulation(button_benchmark_ai, v, f), text="      Start      ", font=("Montserrat", 14))
        button_benchmark_ai.grid(row=1, column=1, sticky="w")
        button_benchmark_ai.bind("<FocusIn>", lambda event: update_button_border(button_benchmark_ai, focused=True))
        button_benchmark_ai.bind("<FocusOut>", lambda event: update_button_border(button_benchmark_ai, focused=False))

        # globals()['label_benchmark_ordinary'] = CTkLabel(frame_benchmark, text="Ordinary T/L:",font=("Montserrat", 20))
        # globals()['label_benchmark_ordinary'].grid(row=3, column=0, pady=(0,20))
        #
        # button_benchmark_ordinary = CTkButton(frame_benchmark, command= lambda v=0, f=frame_benchmark:start_simulation(button_benchmark_ordinary, v, f), text="      Start      ", font=("Montserrat", 14))
        # button_benchmark_ordinary.grid(row=3, column=1, sticky="w", pady=(0,20))
        # button_benchmark_ordinary.bind("<FocusIn>", lambda event: update_button_border(button_benchmark_ordinary, focused=True))
        # button_benchmark_ordinary.bind("<FocusOut>", lambda event: update_button_border(button_benchmark_ordinary, focused=False))

        globals()['label_benchmark_results'] = CTkLabel(frame_benchmark, text="Previous results:",font=("Montserrat", 20))
        globals()['label_benchmark_results'].grid(row=4, column=0)

        globals()['button_benchmark_results'] = CTkButton(frame_benchmark, command=previous_results, text="      View      ", font=("Montserrat", 14))
        globals()['button_benchmark_results'].grid(row=4, column=1, sticky="w")
        globals()['button_benchmark_results'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_benchmark_results'], focused=True))
        globals()['button_benchmark_results'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_benchmark_results'], focused=False))

        globals()['label_benchmark_about'] = CTkLabel(frame_benchmark, text="Note: The duration is settable in settings.", font=("Open Sans", 12), wraplength=(400), justify="left")
        globals()['label_benchmark_about'].grid(row=5, column=0, pady=(0,15), columnspan=2)

        globals()['button_benchmark_back'] = CTkButton(frame_benchmark, text="       Back      ", font=("Montserrat", 16), command=lambda : (return_to_menu(source=frame_benchmark)))
        globals()['button_benchmark_back'].grid(row=6, column=0, columnspan=2)
        globals()['button_benchmark_back'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_benchmark_back'], focused=True))
        globals()['button_benchmark_back'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_benchmark_back'], focused=False))

        benchmark_processed = True
        grid_yes = False

    if grid_yes:
        frame_benchmark.grid(row=0, column=1, ipady=100, ipadx=107, pady=140, padx=(0, 50), sticky="nsew", rowspan=2)
    grid_yes = True
    update_font_size(event, source="benchmark", frame=frame_benchmark)
    # toggle_fullscreen(source="benchmark", frame=frame_benchmark, trigger=False)

    # root.bind("<Configure>", lambda event: update_font_size(event, source="settings"))
    root.bind("<Return>", focus_buttons)
    # root.bind("<F11>", lambda event: toggle_fullscreen(event, "benchmark", frame=frame_benchmark, trigger=True))
    root.bind("<Escape>", lambda event: (return_to_menu(event, source=frame_benchmark)))
    frame_menu.grid_forget()


settings_processed = False
def settings():
    global frame_menu
    global settings_processed
    global grid_yes

    # for widget in frame_menu.winfo_children():
    #     widget.destroy()
    font_size_padding4 = int(root.winfo_width() // 20)  # 50
    time.sleep(0.01)
    if not settings_processed:
        global frame_settings
        frame_settings = CTkFrame(root, corner_radius=25, fg_color="#e7e7e7")
        frame_settings.grid(row=0, column=1, pady=140, padx=(0,font_size_padding4), sticky="nsew", rowspan=2)
        frame_settings.rowconfigure([x for x in range(8)], weight=1)
        frame_settings.columnconfigure([x for x in range(2)], weight=1)

        globals()['label_settings'] = CTkLabel(frame_settings, text="Settings:", font=("Montserrat", 30, "bold"))
        globals()['label_settings'].grid(row=0, column=0, pady=(15, 0))

        globals()['label_settings_density'] = CTkLabel(frame_settings, text="Density:", font=("Montserrat", 20))
        globals()['label_settings_density'].grid(row=1, column=0)

        # options1 = {"Moderate":1, "High":2}
        # selected_option1 = StringVar(value=list(options1.keys())[0])  # Default value is 'Option 2'
        # density_option = CTkOptionMenu(frame_settings, variable=selected_option1  , values=list(options1.keys()), font=("Montserrat", 14), dropdown_font=("Montserrat", 13), anchor="center")
        density_option = CTkLabel(frame_settings, text="The density will automatically change.", font=("Montserrat", 12))
        density_option.grid(row=1, column=1, sticky="w")

        globals()['label_settings_speed'] = CTkLabel(frame_settings, text="Overall Speed:",font=("Montserrat", 20))
        globals()['label_settings_speed'].grid(row=2, column=0)

        default_speed = "Fast"
        default_duration = "16 mins"

        options2 = {"Normal":1.0 , "Fast":2.0}
        selected_option2 = StringVar(value=default_speed)
        speed_option = CTkOptionMenu(frame_settings, variable=selected_option2, values=list(options2.keys()), font=("Montserrat", 14), dropdown_font=("Montserrat", 13), anchor="center", command=lambda _: update_bd_options())
        speed_option.grid(row=2, column=1, sticky="w")

        globals()['label_settings_bd'] = CTkLabel(frame_settings, text="Benchmark duration:",font=("Montserrat", 20))
        globals()['label_settings_bd'].grid(row=3, column=0)

        speed_based_options3 = {
            "Fast": {"16 mins": 960, "32 mins": 1920, "2h": 7200},
            "Normal": {"32 mins": 1920, "64 mins": 3840}
        }
        selected_option3 = StringVar()
        bd_option = CTkOptionMenu(frame_settings, variable=selected_option3, values=[], font=("Montserrat", 14), dropdown_font=("Montserrat", 13), anchor="center")
        bd_option.grid(row=3, column=1, sticky="w")

        # Function to update options based on speed
        def update_bd_options():
            speed = selected_option2.get()
            options3 = speed_based_options3[speed]
            bd_option.configure(values=list(options3.keys()))
            selected_option3.set(list(options3.keys())[0])

            # Set default or fallback selection
            if speed == default_speed:
                selected_option3.set(default_duration)
            else:
                selected_option3.set(list(options3.keys())[0])

        update_bd_options()

        globals()['label_settings_pedestrians'] = CTkLabel(frame_settings, text="Pedestrians:",font=("Montserrat", 20))
        globals()['label_settings_pedestrians'].grid(row=4, column=0)

        globals()['check_settings_pedestrians'] = CTkCheckBox(frame_settings, text="apply people")
        globals()['check_settings_pedestrians'].grid(row=4, column=1, sticky="w", padx=(20,0))
        globals()['check_settings_pedestrians'].select()

        # globals()['label_settings_terminal'] = CTkLabel(frame_settings, text="AI terminal:",font=("Montserrat", 20))
        # globals()['label_settings_terminal'].grid(row=5, column=0)
        #
        # globals()['check_settings_terminal'] = CTkCheckBox(frame_settings, text="show the terminal")
        # globals()['check_settings_terminal'].grid(row=5, column=1, sticky="w", padx=(20,0))

        globals()['label_settings_about'] = CTkLabel(frame_settings, text="Note: Do not forget to save the new values.", font=("Open Sans", 12), wraplength=(350), justify="left")
        globals()['label_settings_about'].grid(row=5, column=0, pady=(0,10), columnspan=2)

        globals()['button_settings_back'] = CTkButton(frame_settings, text="       Back      ", font=("Montserrat", 16), command=lambda : (return_to_menu(source=frame_settings)))
        globals()['button_settings_back'].grid(row=6, column=0)
        globals()['button_settings_back'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_settings_back'], focused=True))
        globals()['button_settings_back'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_settings_back'], focused=False))

        def settings_save():
            globals()['button_settings_save'].configure(text="    Saving ...   ")
            root.after(700, lambda :globals()['button_settings_save'].configure(text="       Save       "))
            root.after(700, lambda: globals()['label_settings_about'].configure(text="The new values have been saved."))

            # density = options1[selected_option1.get()]
            overall_speed = options2[selected_option2.get()]
            current_speed = selected_option2.get()
            duration = speed_based_options3[current_speed][selected_option3.get()]
            pedestrians = globals()['check_settings_pedestrians'].get()
            # terminal = globals()['check_settings_terminal'].get()
            number = random.randint(1000, 9999)

            settings_values.update({'Overall_speed':overall_speed, 'Duration':duration, 'Pedestrians': pedestrians, 'randomness_seed':number})

            with open(file_path0, 'w') as file:
                json.dump(settings_values, file)

        globals()['button_settings_save'] = CTkButton(frame_settings, text="       Save       ", font=("Montserrat", 16), command=settings_save)
        globals()['button_settings_save'].grid(row=6, column=1)
        globals()['button_settings_save'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_settings_save'], focused=True))
        globals()['button_settings_save'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_settings_save'], focused=False))

        settings_processed = True
        grid_yes = False

    if grid_yes:
        frame_settings.grid(row=0, column=1, pady=140, padx=(0, font_size_padding4), sticky="nsew", rowspan=2)
    grid_yes = True
    update_font_size(event, source="settings", frame=frame_settings)
    # toggle_fullscreen(source="settings", frame=frame_settings, trigger=False)

    # root.bind("<Configure>", lambda event: update_font_size(event, source="settings"))
    root.bind("<Return>", focus_buttons)
    # root.bind("<F11>", lambda event: toggle_fullscreen(event, "settings", frame=frame_settings, trigger=True))
    root.bind("<Escape>", lambda event: (return_to_menu(event, source=frame_settings)))
    frame_menu.grid_forget()

def exit_confirmation(event=None):
    if globals()['exit_confirmation_window_open']:
        return

    globals()['exit_confirmation_window_open'] = True

    root_width = root.winfo_width()
    root_height = root.winfo_height()
    root_x = root.winfo_x()
    root_y = root.winfo_y()

    window_width = int(root.winfo_screenwidth() / 6.4)
    window_height = int(root.winfo_screenheight() / 7.2)

    x = root_x + (root_width // 2) - (window_width // 2)
    y = root_y + (root_height // 2) - (window_height // 2)

    new_window = CTkToplevel(root)
    new_window.geometry(f"{window_width}x{window_height}+{x}+{y}")
    new_window.title("Exit Confirmation")
    new_window.resizable(False, False)

    for i in range(2, 7):
        if globals()[f'button{i}'].winfo_exists():
            globals()[f'button{i}'].configure(state="disabled")

    CTkLabel(new_window,
             text="Are you sure you want to exit?",
             font=("Calibri", 14, "bold")
             )\
        .pack(pady=20)

    globals()['button_yes'] = CTkButton(new_window,
                           text="Yes",
                           font=("Montserrat", 12),
                           width=80,
                           command=lambda : root.destroy()
                           )
    globals()['button_yes'].pack(side="left", pady=10, padx=(40, 5))
    globals()['button_yes'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_yes'], focused=True))
    globals()['button_yes'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_yes'], focused=False))

    def check_button_state(event=None):
        for i in range(2, 7):
            if globals()[f'button{i}'].winfo_exists():
                globals()[f'button{i}'].configure(state="normal")

        root.attributes("-disabled", False)
        new_window.destroy()

    globals()['button_no'] = CTkButton(new_window,
                          text="No",
                          font=("Montserrat", 12),
                          width=80,
                          command=check_button_state
                          )
    globals()['button_no'].pack(side="right", pady=5, padx=(5, 40))
    globals()['button_no'].bind("<FocusIn>", lambda event: update_button_border(globals()['button_no'], focused=True))
    globals()['button_no'].bind("<FocusOut>", lambda event: update_button_border(globals()['button_no'], focused=False))

    def set_default_button(_event=None):
        globals()['button_yes'].invoke()

    def shake_window(_event=None):
        """Make the window shake by moving it left and right."""
        if globals()['shaking']:
            return
        globals()['shaking']=True
        original_close_command = new_window.protocol("WM_DELETE_WINDOW")  # Save the original command
        new_window.protocol("WM_DELETE_WINDOW", lambda: None)  # Replace with a no-op
        new_window.after(300, lambda: new_window.protocol("WM_DELETE_WINDOW", original_close_command))

        globals()['button_no'].configure(state="disabled")
        globals()['button_yes'].configure(state="disabled")
        # new_window.unbind("<Escape>")

        original_x = new_window.winfo_x()  # Get the current X position of the window
        shake_distance = 5  # How far to move the window
        shake_duration = 0.3  # Duration of the shake (in seconds)
        end_time = time.time() + shake_duration

        while time.time() < end_time:
            # Move left
            new_window.geometry(f"+{original_x - shake_distance}+{new_window.winfo_y()}")
            new_window.update()
            time.sleep(0.05)

            # Move right
            new_window.geometry(f"+{original_x + shake_distance}+{new_window.winfo_y()}")
            new_window.update()
            time.sleep(0.05)

        # Return to the original position
        new_window.geometry(f"+{original_x}+{new_window.winfo_y()}")

        globals()['button_no'].configure(state="normal")
        globals()['button_yes'].configure(state="normal")
        def wait_time():
            globals()['shaking'] = False
            new_window.bind("<Escape>", check_button_state)

        new_window.after(1500, wait_time)

    def unbindding_Escape(event):
        new_window.focus_set()
        new_window.unbind("<Escape>")
        new_window.after(200, shake_window)

    globals()['shaking'] = False
    new_window.bind("<Button-1>", unbindding_Escape)
    new_window.focus_force()
    # new_window.bind("<Return>", set_default_button)
    new_window.bind("<Escape>", lambda event:root.after(100, check_button_state))
    new_window.bind("<Return>", focus_buttons)

    # root.attributes("-disabled", True) # warning effect as clicked on root, but it has problems

    new_window.protocol("WM_DELETE_WINDOW", check_button_state)

    new_window.transient(root)  # Make the window modal
    new_window.grab_set()  # Force interaction on this window only
    # root.attributes("-topmost", True)
    # new_window.attributes("-topmost", True)
    new_window.wait_window()    # Block further code until the window is closed
    #root.attributes("-disabled", False)
    globals()['exit_confirmation_window_open'] = False

def focus_buttons(event):
        focused_widget = root.focus_get()  # Get the currently focused widget
        # print(f"Key Pressed: {event.keysym}, Focused Widget: {focused_widget}")

        # Resolve the actual button if the focus is on a subcomponent
        while focused_widget and not isinstance(focused_widget, CTkButton):
            focused_widget = focused_widget.master

        if isinstance(focused_widget, CTkButton) and event.keysym == "Return":
            focused_widget.invoke()  # Trigger the button command

def update_button_border(button, focused=False):
    """Update the button border width when focused or unfocused."""
    if focused:
        button.configure(fg_color="#36719f")  # Highlight border when focused
    else:
        button.configure(fg_color="#3b8ed0")  # Normal border

def update_font_size(event, source=None, frame=None):
    screen_width = root.winfo_width()
    screen_height = root.winfo_height()

    window_width = float((root.winfo_width() / 100) / 20) # 0.5

    font_size_textbox = 14
    font_size_buttons = 14
    font_size_label0 = 32
    font_size_label1 = 24
    font_size_label2 = 10
    font_size_esc_press = 18
    font_size_padding0 = 2
    font_size_padding1 = 20
    font_size_padding2 = 18
    font_size_padding3 = 16
    font_size_padding4 = 50
    font_size_padding5 = 100
    font_size_padding6 = 107
    font_size_padding7 = 400
    font_size_padding8 = 166
    font_size_padding9 = 18
    font_size_padding10 = 22
    font_size_padding11 = 21
    font_size_padding12 = 3
    font_size_padding13 = 17

    formula_padding0 = 20  # 1920 = 4, 1000=44
    formula_padding1 = 22  # 1920 = 30, 1000=46
    formula_padding2 = 8  # 1920 = 4, 1000=32
    formula_padding3 = 8  # 1920 = 5, 1000=32


    frame_about.grid_configure(ipady=font_size_padding5, padx=(font_size_padding4, font_size_padding4))
    frame_bg.grid_configure(padx=(100, 100))
    globals()['textbox_about'].configure(font=("Open Sans", font_size_textbox))
    globals()['textbox_about'].grid_configure(padx=font_size_padding3 , pady=font_size_padding3)
    globals()["scroll_speed"] = window_width

    for widget in frame.winfo_children():  # Loop through all widgets
        if isinstance(widget, CTkButton):  # Check if it's a CTkButton
            widget.configure(
                font=("Montserrat", font_size_buttons),
                corner_radius=font_size_padding3,
            )
        if isinstance(widget, CTkOptionMenu):  # Check if it's a CTkButton
            widget.configure(
                font=("Open Sans", font_size_textbox),
                width=font_size_padding8,
                height=font_size_padding10,
                dropdown_font=("Montserrat", font_size_textbox),
                corner_radius=font_size_padding2
            )



    if source == "show_menu_content":
        frame_menu.grid_configure(padx=(0, font_size_padding4))
        globals()['label0'].configure(font=("Montserrat", font_size_padding11, "bold"))
        globals()['label1'].configure(font=("Montserrat", font_size_padding9, "bold"))
        # globals()['label2'].configure(font=("Arial", font_size_label2))
        globals()['button6'].grid_configure(pady=(font_size_padding0, 0))
        globals()['label2'].grid_configure(pady=(font_size_padding1, font_size_padding2))

    if source == "ai_based_traffic_light":
        frame_ai.grid_configure(padx=(0, font_size_padding4), ipadx=formula_padding2)
        globals()['label_ai'].configure(font=("Montserrat", font_size_padding11, "bold"))
        globals()['label_ai1'].configure(font=("Montserrat", font_size_padding1))
        globals()['label_ai_about'].configure(font=("Open Sans", font_size_label2), wraplength=(font_size_padding7))

    elif source == "ordinary_traffic_light":
        frame_ordinary.grid_configure(padx=(0, font_size_padding4), ipadx=formula_padding3)
        globals()['label_ordinary'].configure(font=("Montserrat", font_size_padding11, "bold"))
        globals()['label_ordinary1'].configure(font=("Montserrat", font_size_padding1))
        globals()['label_ordinary_about'].configure(font=("Open Sans", font_size_label2), wraplength=(font_size_padding7))

    elif source == "benchmark":
        frame_benchmark.grid_configure(padx=(0, font_size_padding4), ipadx=formula_padding1)
        globals()['label_benchmark'].configure(font=("Montserrat", font_size_label0, "bold"))
        globals()['label_benchmark_ai'].configure(font=("Montserrat", font_size_padding1))
        # globals()['label_benchmark_ordinary'].configure(font=("Montserrat", font_size_padding1))
        globals()['label_benchmark_results'].configure(font=("Montserrat", font_size_padding1))
        globals()['label_benchmark_about'].configure(font=("Open Sans", font_size_label2), wraplength=(font_size_padding7))

    elif source == "settings":
        frame_settings.grid_configure(padx=(0, font_size_padding4), ipadx=formula_padding0)
        globals()['label_settings'].configure(font=("Montserrat", font_size_label0, "bold"))
        globals()['label_settings_density'].configure(font=("Montserrat", font_size_padding1))
        globals()['label_settings_speed'].configure(font=("Montserrat", font_size_padding1))
        globals()['label_settings_bd'].configure(font=("Montserrat", font_size_padding13))
        globals()['label_settings_pedestrians'].configure(font=("Montserrat", font_size_padding1))
        globals()['check_settings_pedestrians'].configure(font=("Open Sans", font_size_label2), checkbox_width=font_size_padding10, checkbox_height=font_size_padding10, border_width=font_size_padding12)
        globals()['check_settings_pedestrians'].grid_configure(padx=(font_size_padding10,0))
        # globals()['label_settings_terminal'].configure(font=("Montserrat", font_size_padding1))
        # globals()['check_settings_terminal'].configure(font=("Open Sans", font_size_label2), checkbox_width=font_size_padding10, checkbox_height=font_size_padding10, border_width=font_size_padding12)
        # globals()['check_settings_terminal'].grid_configure(padx=(font_size_padding10,0))
        globals()['label_settings_about'].configure(font=("Open Sans", font_size_label2), wraplength=(font_size_padding7), text="Note: Do not forger to save the new values.")

current_source = "show_cotent_menu"
# def toggle_fullscreen(event=None, source=None, frame=None, trigger=False):
#     global current_source
#
#     current_frame = frame_menu
#     if not frame == None:
#         current_frame = frame
#
#     if not source == None:
#         current_source = source
#
#     """Toggle fullscreen mode."""
#     if root.attributes("-fullscreen") and trigger == True:
#         # root.unbind("<F11>")
#         # root.after(1000, lambda : root.bind("<F11>", toggle_fullscreen))
#         root.attributes("-fullscreen", False)
#         root.resizable(False, False)
#         update_font_size(event, current_source, frame=current_frame)
#         for widget in root.winfo_children():
#             canvas.place_forget()
#         def full():
#             create_canvas()
#         root.after(10, full)
#
#     else:
#         if trigger == True:
#             # root.unbind("<F11>")
#             # root.after(500, lambda : root.bind("<F11>", toggle_fullscreen))
#             root.attributes("-fullscreen", True)
#             update_font_size(event, current_source, frame=current_frame)
#             # Remove size restrictions in fullscreen mode
#             root.minsize(1, 1)  # Allow fullscreen dimensions
#             root.maxsize(10000, 10000)  # Effectively no max size limit
#             for widget in root.winfo_children():
#                 canvas.place_forget()
#             def full():
#                 create_canvas()
#             root.after(10, full)

def create_canvas(source=None):
    global scroll_background
    def scroll_background():
        """Move the background images to create a rolling effect."""
        canvas.move(bg1, -scroll_speed, 0)
        canvas.move(bg2, -scroll_speed, 0)

        # Get the current position of the images
        bg1_coords = canvas.coords(bg1)
        bg2_coords = canvas.coords(bg2)

        # If an image goes out of view, reset its position
        if bg1_coords[0] <= -image_width:
            canvas.move(bg1, 2 * image_width, 0)
        if bg2_coords[0] <= -image_width:
            canvas.move(bg2, 2 * image_width, 0)

        # Repeat the scrolling after a short delay
        root.after(20, scroll_background)


    def create_blurry_bg(image_path, blur_radius=1):
        """Load an image, apply a blur effect, and return a PhotoImage."""
        try:
            # Open the image
            image = Image.open(image_path)
            size = (root.winfo_width(), root.winfo_height())  # Resize to fit the window
            image = image.resize(size)

            # Apply a blur filter
            blurred_image = image.filter(ImageFilter.GaussianBlur(radius=blur_radius))

            # Convert to ImageTk.PhotoImage
            return ImageTk.PhotoImage(blurred_image)
        except Exception as e:
            print(f"Error applying blur: {e}")
            return None

    canvas.place(x=0, y=0, relwidth=1, relheight=1)
    # Load the background image
    image_path = os.path.join(os.path.dirname(__file__), "Untitled design5.png")

    global bg_image_bullury  # Prevent garbage collection
    bg_image_bullury = create_blurry_bg(image_path)

    if not bg_image_bullury:
        print("Failed to load the background image.")
        return

    # Get screen dimensions
    global image_width, image_height
    image_width = root.winfo_width()
    image_height = root.winfo_height()

    # Add two copies of the image to the canvas for seamless scrolling
    global bg1, bg2 # Prevent garbage collection
    bg1 = canvas.create_image(0, 0, anchor="nw", image=bg_image_bullury)
    bg2 = canvas.create_image(image_width, 0, anchor="nw", image=bg_image_bullury)
    scroll_speed = globals()["scroll_speed"]

def center_text(text, width=50):
    lines = text.split("\n")
    centered_text = ""

    for line in lines:
        # Calculate spaces to center-align the line
        line_length = len(line)
        if line_length < width:
            spaces_needed = (width - line_length) // 2
            centered_line = " " * spaces_needed + line
            centered_text += centered_line + "\n"
        else:
            centered_text += line + "\n"

    return centered_text

def return_to_menu(event=None, source=None):
    frame_menu.grid(row=0, column=1, ipady=100, ipadx=70, pady=140, padx=(0,50), sticky="nsew", rowspan=2)
    root.bind("<Escape>", exit_confirmation)
    update_font_size(event, source="show_menu_content", frame=frame_menu)
    # root.bind("<F11>", lambda event: toggle_fullscreen(event, "show_menu_content", frame=frame_menu, trigger=True))
    root.after(50, lambda :source.grid_forget())

    # root.bind("<Configure>", lambda event: (update_font_size(event, source="show_menu_content")))

    # for widget in source.winfo_children():
    #     widget.destroy()
    # show_menu_content()

def show_menu_content(_event=None):
    global frame_menu
    frame_menu = CTkFrame(root, corner_radius=25, fg_color="#e7e7e7")
    frame_menu.grid(row=0, column=1, ipady=100, ipadx=70, pady=140, padx=(0,50), sticky="nsew", rowspan=2)
    frame_menu.rowconfigure([x for x in range(6)], weight=1)
    frame_menu.columnconfigure(0, weight=1)

    # for widget in root.winfo_children():
    #     if widget == canvas or widget == frame_about or widget == frame_menu:
    #         continue
    #     widget.destroy()

    globals()['label0'] = CTkLabel(
        frame_menu,
        text="Artificial Intelligence - Traditional",
        font=("Open Sans",32,"bold"),
        wraplength=(2000)
        )
    globals()['label0'].grid(row=0, column=0, sticky="nsew", padx=10, pady=(20,0))

    globals()['label1'] = CTkLabel(
        frame_menu,
        text="traffic system simulation",
        font=("Open Sans",24,"bold"),
        wraplength=(2000)
        )
    globals()['label1'].grid(row=1, column=0, sticky="nsew", pady=(0, 15))

    globals()['label2'] = CTkLabel(
        frame_menu,
        # text="Press F11 to toggle fullscreen mode on and off. ",
        text=" ",
        font=("Arial", 10),
        wraplength=(2000)
        )
    globals()['label2'].grid(row=7, column=0, sticky="nsew", padx=10)

    button_names = [
        "  AI-based traffic light  ",
        "   Normal traffic light   ",
        "         Benchmark          ",
        "             Settings            ",
        "                Exit                 "
    ]

    button_functions = [
        ai_based_traffic_light,
        ordinary_traffic_light,
        benchmark,
        settings,
        lambda : sys.exit()]

    for i, (name, func) in enumerate(zip(button_names, button_functions),start=2):
        globals()[f'button{i}']=CTkButton(
            frame_menu,
            text=name,
            command=func,
            font = ("Montserrat", 18),
            corner_radius = 10,
            text_color="white",
            border_width=0,
            border_color="#70a5d0"
            )
        globals()[f'button{i}'].grid(row=i, column=0, ipady=2.5)
        button_menu = globals()[f'button{i}']
        button_menu.bind("<FocusIn>", lambda event, bm=button_menu: update_button_border(bm, focused=True))
        button_menu.bind("<FocusOut>", lambda event, bm=button_menu: update_button_border(bm, focused=False))
    globals()['button6'].grid_configure(pady=(8, 0))

    root.bind("<Return>", focus_buttons)
    update_font_size(event, source="show_menu_content", frame=frame_menu)
    # root.bind("<F11>", lambda event: toggle_fullscreen(event, "show_menu_content", frame=frame_menu, trigger=True))
    # root.bind("<Configure>", lambda event: (update_font_size(event, source="show_menu_content")))
    root.bind("<Escape>", exit_confirmation)


root = CTk()
root.title("AI-TR traffic system simulation by Hamidreza")
predefined_height = int(root.winfo_screenheight() / 1.49)
predefined_width = int(root.winfo_screenwidth() / 1.92)
# print(predefined_height, predefined_width)
root.geometry(f"{predefined_width}x{predefined_height}")
root.resizable(False, False)
set_appearance_mode("Light")
#root.configure(fg_color="#f3f3f3")
root.configure(fg_color="#ffffff")
#root.minsize(820, 560)


root.rowconfigure([x for x in range(2)], weight=1)
root.columnconfigure([x for x in range(2)], weight=1)

def resource_path(relative_path):
    base_path = getattr(sys, '_MEIPASS', os.path.abspath("."))
    return os.path.join(base_path, relative_path)

# Icon = os.path.join(os.path.dirname(__file__), "logo1.ico")
icon_path = resource_path("logo2.ico")
root.iconbitmap(icon_path)

globals()['exit_confirmation_window_open'] = False
root.bind("<Button-1>", lambda event: root.focus_set())

# image_path = "background.jpg"
# image_open = Image.open(image_path)
# main_bg = CTkImage(light_image=image_open, dark_image=image_open, size = (1000, 725))
# bg_pic = CTkLabel(root, image=main_bg, text="")
# bg_pic.place(x=0, y=0, relwidth=1, relheight=1) # For adding BG IMAGE also edit Update

canvas_width = predefined_width
canvas_height = predefined_height
canvas = tk.Canvas(root, width=canvas_width, height=canvas_height)

global frame_bg
frame_bg = CTkFrame(root, corner_radius=25, fg_color="#e7e7e7")
frame_bg.grid(row=0, column=1, ipady=100, ipadx=90, pady=140, padx=(0, 50), sticky="nsew", rowspan=2)


global frame_about
frame_about = CTkFrame(root, corner_radius=25, fg_color="#cacaca")
frame_about.grid(row=0, column=0, ipady=100, pady=140, padx=(50, 50), sticky="nsew", rowspan=2)
frame_about.rowconfigure(0, weight=1)
frame_about.columnconfigure(0, weight=1)

about_text = (
            "*** For best results and prevent errors during simulation"
            " try to  turning off power-saving mode. If you're using a laptop,"
            " it's recommended to connect the power cable to maintain consistent 60 FPS. \n"
            "If you experience performance issues, consider setting the Overall Speed to Normal   \n"

            
            "\nThis is a comparison between traditional and AI-based"
            " traffic systems at a four-way intersection under equal conditions. \n"
            
            "\nTraditional: Uses clockwise lane selection with a fixed 30-second green light duration per lane. \n"
            
            "\nAI-based: Lane selection and green light duration"
            " are dynamically optimized based on traffic density, the lane, and overall density,"
            " lane demand, emergency vehicle presence, and individual lane wait times. \n"
            
            "\nNote: The AI-based green light durations  range between 10 to 35 seconds."
            " Emergency vehicles include ambulances, fire trucks, and white police cars. \n"
            
            "\nYou can press 1, 2, 3, 4, 5, or 6 to switch camera angles during the simulation. \n"
            
            "\nThe AI system is trained on over 3,000 samples using a Reinforcement Learning approach"
            " (reward and penalty-based learning). \n"
            
            "\nTo implement this AI-based traffic system in the real world, each lane would require"
            " equipment such as cameras with computer vision capabilities and sensors to detect"
            "vehicles beyond the camera's field of view. \n "

            "\nSpecial Thanks: The 3D environment in this project is based on the Urban Traffic System (UTS) by AGLOBEX. "
            "All rights remain with the original creator."
            )

globals()['textbox_about'] = CTkTextbox(
    frame_about,
    activate_scrollbars=False,
    corner_radius=25,
    font=("Open Sans", 16),
    fg_color="#e7e7e7",
    wrap="word"
)
globals()['textbox_about'].grid(row=0, column=0, sticky="nsew", pady=10, padx=10)

v_scroll_text = tk.Scrollbar(frame_about, orient="vertical")
# v_scroll_text.grid()
globals()['textbox_about'].configure(yscrollcommand=v_scroll_text.set)

centered_text = center_text(about_text, width=50)
globals()['textbox_about'].insert("0.5", centered_text)

globals()['textbox_about'].configure(state="disabled", cursor="arrow")
for event in ["<Button-1>", "<B1-Motion>", "<Control-c>", "<Control-x>", "<Control-a>", "<ButtonRelease-1>"]:
    globals()['textbox_about'].bind(event, lambda e: "break")

number = random.randint(1000, 9999)

settings_values = {'Overall_speed': 2.0, 'Duration': 960, 'Pedestrians': 1, 'Benchmarking': 3, 'randomness_seed':number}

# Get the actual (current) Documents folder path
# docs_path = shell.SHGetFolderPath(0, shellcon.CSIDL_PERSONAL, None, 0)

# Build the full path to AI/Traffic inside Documents
# traffic_path = os.path.join(docs_path, "AI traffic system")

def get_documents_folder():
    # Uses Windows API to get real Documents path
    CSIDL_PERSONAL = 5
    SHGFP_TYPE_CURRENT = 0
    buf = ctypes.create_unicode_buffer(ctypes.wintypes.MAX_PATH)
    ctypes.windll.shell32.SHGetFolderPathW(None, CSIDL_PERSONAL, None, SHGFP_TYPE_CURRENT, buf)
    return buf.value

# Define destination folder and full file path
folder = os.path.join(get_documents_folder(), "AI-TR traffic system simulation")
# Create the folder if it doesn't exist
os.makedirs(folder, exist_ok=True)

file_path0 = os.path.join(folder, "settings.json")
file_path1 = os.path.join(folder, "TheLatestReturnedData.json")
file_path2 = os.path.join(folder, "PreviousResults.json")
file_path3 = os.path.join(folder, "shared_data.txt")
file_path4 = os.path.join(folder, "duration_agent.pth")
file_path5 = os.path.join(folder, "AI traffic system")
file_path6 = os.path.join(folder, "PreviousResults.json")


if os.path.exists(file_path4):
    pass
else:
    src = resource_path("duration_agent.pth")
    dst = os.path.join(folder, "duration_agent.pth")
    shutil.copyfile(src, dst)

if os.path.exists(file_path6):
    pass
else:
    src = resource_path("PreviousResults.json")
    dst = os.path.join(folder, "PreviousResults.json")
    shutil.copyfile(src, dst)

if os.path.exists(file_path1):
    os.remove(file_path1)

if os.path.exists(file_path3):
    os.remove(file_path3)
with open(file_path0, 'w') as file:
    json.dump(settings_values, file)

print("Here, you can see what the AI agent is doing internally.")

def start():
    create_canvas()
    scroll_background()

root.after(100, start)

show_menu_content()

try:
    root.mainloop()
except KeyboardInterrupt:
    print("\nKeyboardInterrupt detected. Performing cleanup...")
