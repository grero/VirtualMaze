
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using VirtualMaze.Assets.Scripts.Raycasting;


/// <summary>
/// MonoBehaviour that affects VirtualMaze globally.
/// </summary>
public class GameController : MonoBehaviour {
    //UPDATE THESE WITH EACH COMPILATION
    public static readonly int versionNum = 4;
    public static readonly string versionInfo = "Taxi Continuous/Discontinuous v";
    public static readonly string pportInfo = "v" + versionNum;

    [SerializeField]
    private ScreenSaver saver = null;

    private bool generationComplete = false;

    private string SessionPattern = "[Ss]ession[0-9]{1}";
    private string DayPattern = "[0-9]{8}";

    private string eyelinkMatFile = $"{Path.DirectorySeparatorChar}eyelink.mat";
    private string unityfileMatFile = $"{Path.DirectorySeparatorChar}unityfile.mat";
    private string resultFile = $"{Path.DirectorySeparatorChar}unityfile_eyelink_new.csv";

    private static GameController _instance;
    public static GameController instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType(typeof(GameController)) as GameController;
                if (_instance == null) {
                    Debug.LogError("need at least one GameController");
                }
            }
            return _instance;
        }
    }

    private void Update() {
        ProcessKeyPress();
    }

    //framerate dependent
    private readonly int pressDelay = 10;
    private int counter = 10;

    private void ProcessKeyPress() {
        if (!Application.isBatchMode && Input.GetKey(KeyCode.Escape)) {
            if (counter > 0) {
                counter--;
            }
            else {
                Application.Quit();
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape)) {
            counter = pressDelay;
        }
    }



    private void Start() {
        
        //Online sources says that if vSyncCount != 0, targetFrameRate will be ignored.
        if (!Application.isEditor) {
            Application.targetFrameRate = 30;
            /* if display is 60hz, Unity will run at 30hz */
            QualitySettings.vSyncCount = 1;
        }

        if (Application.isBatchMode) {
            Camera.main.aspect = (1920f/1080f);
            Debug.LogError($"Camera aspect ratio : {Camera.main.aspect}");
            Camera camera = saver.viewport;
            float fov = camera.fieldOfView;
            var frustrumHeight = 2.0f * 25 * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            var frustrumWidth = frustrumHeight * camera.aspect;
            Debug.LogError($"Distance at 25 units (horz) : {frustrumWidth}");
            BatchModeLogger logger = new BatchModeLogger(PresentWorkingDirectory);

            string[] args = Environment.GetCommandLineArgs();


            Queue<DirectoryInfo> dirQ = new Queue<DirectoryInfo>();
            bool isSessionList = false;

            float radius = RaycastSettings.DefaultGazeRadius;
            float stepSize = RaycastSettings.DefaultStepSize;
            float distToScreen = RaycastSettings.DefaultDistToScreen;
            float screenX = RaycastSettings.DefaultScreenCmDimX;
            float screenY = RaycastSettings.DefaultScreenCmDimY;
            float resX = RaycastSettings.DefaultScreenPixelDimX;
            float resY = RaycastSettings.DefaultScreenPixelDimY;

            string sessionListPath = null;


            for (int i = 0; i < args.Length; i++) {
                Debug.LogError($"ARG {i}: {args[i]}");

                switch (args[i].ToLower()) {
                    case "-sessionlist":
                        isSessionList = true;
                        logger.Print($"Session List detected!");
                        sessionListPath = args[i + 1];
                        break;
                    case "-stepsize":
                        if (float.TryParse(args[i + 1], out stepSize)) {
                            logger.Print($"Setting stepsize to: {stepSize}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to float, using {stepSize} as default");
                        }
                        break;
                    case "-radius":
                        if (float.TryParse(args[i + 1], out radius)) {
                            logger.Print($"Setting radius to: {radius}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to float, using {radius} as default");
                        }
                        break;
                    case "-disttoscreen":
                        if (float.TryParse(args[i + 1], out distToScreen)) {
                            logger.Print($"Setting distToScreen to: {distToScreen}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to float, using {distToScreen} as default");
                        }
                        break;
                    case "-screenx":
                        if (float.TryParse(args[i + 1], out screenX)) {
                            logger.Print($"Setting screenX to: {screenX}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to float, using {screenX} as default");
                        }
                        break;
                    case "-screeny":
                        if (float.TryParse(args[i + 1], out screenY)) {
                            logger.Print($"Setting screenY to: {screenY}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to float, using {screenY} as default");
                        }
                        break;
                    case "-resx":
                        if (float.TryParse(args[i + 1], out resX)) {
                            logger.Print($"Setting resX to: {resX}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to float, using {resX} as default");
                        }
                        break;
                    case "-resy":
                        if (float.TryParse(args[i + 1], out resY)) {
                            logger.Print($"Setting resY to: {resY}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to float, using {resY} as default");
                        }
                        break;
                }
            }
            RaycastSettings raycastSettings = RaycastSettings.FromFloat(
                distToScreen : distToScreen,
                gazeRadius : radius,
                stepSize : stepSize,
                screenCmX : screenX, 
                screenCmY : screenY, 
                screenPixelX : resX, 
                screenPixelY : resY);

            if (!isSessionList) {
                PwdMode(logger, dirQ);
            }
            else {
                SessionListMode(logger, sessionListPath, dirQ);
            }
            //BinWallManager.ReconfigureGazeOffsetCache(radius, density);
            ProcessExperimentQueue(dirQ, logger, raycastSettings);
        }
    }

    private void SessionListMode(BatchModeLogger logger, string listPath, Queue<DirectoryInfo> dirQ) {
        using (StreamReader reader = new StreamReader(listPath)) {

            while (reader.Peek() > 0) {
                DirectoryInfo dir = new DirectoryInfo(reader.ReadLine());
                dirQ.Enqueue(dir);
            }
        }
    }

    private void PwdMode(BatchModeLogger logger, Queue<DirectoryInfo> dirQ) {
        DirectoryInfo pwd = new DirectoryInfo(PresentWorkingDirectory);
        dirQ.Enqueue(pwd);
    }

    private void ProcessExperimentQueue(Queue<DirectoryInfo> dirQ, BatchModeLogger logger, RaycastSettings raycastSettings) {
        Queue<string> sessionQ = new Queue<string>();

        while (dirQ.Count > 0) {
            DirectoryInfo dir = dirQ.Dequeue();
            if (IsDayDir(dir)) {
                IEnumerable<string> subDirs = Directory.EnumerateDirectories(dir.FullName, "*", SearchOption.TopDirectoryOnly);
                foreach (string subDir in subDirs) {
                    if (IsSessionDir(new DirectoryInfo(subDir))) {
                        logger.Print($"Queuing {subDir}");
                        
                        sessionQ.Enqueue(subDir);
                    }
                }
            }
            else if (IsSessionDir(dir)) {
                logger.Print($"Queuing {dir}");
                sessionQ.Enqueue(dir.FullName);
            }
        }

        //BinMapper mapper = new DoubleTeeBinMapper(numOfBinsForFloorLength);

        if (sessionQ.Count > 0) {
            logger.Print($"{sessionQ.Count} sessions to be processed");
            ProcessSession(sessionQ, logger, raycastSettings);
        }
        else {
            logger.Print("No Session directories found! Exiting");
            logger.Dispose();
            Application.Quit();
        }
    }

private string GetSessionFilePath(string sessionDir) {
        // Look for any session*.txt file
        
        string[] txtFiles = Directory.GetFiles(sessionDir, "session*.txt");
        
        if (txtFiles.Length > 0) {
            Debug.LogError($"Found .txt session file: {txtFiles[0]}");
            //return txtFiles[0];
        }
        string matPath = sessionDir + unityfileMatFile;
        if (File.Exists(matPath)) {
            Debug.LogError($"Found .mat session file: {matPath}");
            return matPath;
        }
        
        Debug.LogError($"No session file found in {sessionDir}");
        return null;
    }
    private async void ProcessSession(Queue<string> sessions, BatchModeLogger logger, RaycastSettings raycastSettings)
    {
        string path;
        int total = sessions.Count;
        int count = 1, notifyAliveCount = 0;

        while (sessions.Count > 0)
        {
            path = sessions.Dequeue();
            logger.Print($"Starting({count}/{total}): {path}");
            generationComplete = false;

            string sessionFilePath = GetSessionFilePath(path);
            if (sessionFilePath == null)
            {
                logger.Print($"Failed: No valid session file found in {path}");
                count++;
                continue;
            }

            logger.Print($"Path: {path}");
            logger.Print($"Session file: {sessionFilePath}");


            StartCoroutine(ProcessWrapper(sessionFilePath, path + eyelinkMatFile, path, logger, raycastSettings));
            while (!generationComplete)
            {
                await Task.Delay(10000); //10 second notify-alive message

                notifyAliveCount++;
                notifyAliveCount %= 6; //only print message every 60 seconds
                if (notifyAliveCount == 0)
                {
                    logger.Print($"{saver.progressBar.value * 100}%: Data Generation is still running. {DateTime.Now.ToString()}");
                }
            }
            if (File.Exists(path + resultFile))
            {
                if (saver.progressBar.value != 1)
                {
                    logger.Print($"Exited but not finished, check debug logger for possible reason!");
                    logger.Print($"Percentage completed : {saver.progressBar.value * 100}%");
                }
                logger.Print($"Success: {path + resultFile}");
            }
            else
            {
                logger.Print($"Failed: {path + resultFile}. Add to the command \"-logfile <log file location>.txt\" to debug");
            }
            count++;
        }
        logger.Print("BatchMode Complete! Exiting VirtualMaze.");
        logger.Dispose();
        Application.Quit();
    }

    private string PresentWorkingDirectory { get => Path.GetFullPath("."); }

    private bool IsDayDir(DirectoryInfo dirInfo) {
        return Regex.IsMatch(dirInfo.Name, DayPattern);
    }

    private bool IsSessionDir(DirectoryInfo dirInfo) {
        return Regex.IsMatch(dirInfo.Name, SessionPattern);
    }

    private IEnumerator ProcessWrapper(string sessionPath, string edfPath, string toFolderPath, BatchModeLogger logger, RaycastSettings raycastSettings) {
        print($"session: {sessionPath}");
        print($"edf: {edfPath}");
        print($"toFolder: {toFolderPath}");




        try {
            yield return saver.ProcessSessionDataTask(sessionPath, edfPath, toFolderPath, raycastSettings);
        }
        finally { //so that the batchmode app will quit or move on the the next session
            generationComplete = true;
        }

    }
}
