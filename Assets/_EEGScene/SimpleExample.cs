using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmotivUnityPlugin;
using UnityEngine.UI;

public class SimpleExample : MonoBehaviour
{
    
    // Please fill clientId and clientSecret of your application before starting
    private string _clientId = "XNi4mg74a8ucHFfXMBZxrqWexERjBgdRWsBSkfyM";
    private string _clientSecret = "EMUp4dEVwkvoYv4CBj9Kne40VlKwejMZ25ZxCFRrZmfEO1RQ0jzwcZ1JVhGL560zjicvZF8lNn5fcC1rfce6XTwxEZMN1cf3drOGHbXABCUDMglO0ARgHW5OGqnKx5Ag";
    private string _appName = "UnityApp";
    private string _appVersion = "3.3.0";

    EmotivUnityItf _eItf = EmotivUnityItf.Instance;
    float _timerDataUpdate = 0;
    const float TIME_UPDATE_DATA = 1f;
    bool _isDataBufferUsing = false; // default subscribed data will not saved to Data buffer


    [SerializeField] public InputField  HeadsetId;   // headsetId
    [SerializeField] public InputField  RecordTitle;     // record Title
    [SerializeField] public InputField  RecordDescription;     // record description
    [SerializeField] public InputField  ProfileName;   // headsetId

    [SerializeField] public Dropdown ActionNameList;

    [SerializeField] public InputField  MarkerValue;     // marker value
    [SerializeField] public InputField  MarkerLabel;     // marker Label
    [SerializeField] public Toggle EEGToggle;
    [SerializeField] public Toggle MOTToggle;
    [SerializeField] public Toggle PMToggle;
    [SerializeField] public Toggle CQToggle;
    [SerializeField] public Toggle POWToggle;
    [SerializeField] public Toggle EQToggle;
    [SerializeField] public Toggle COMToggle;
    [SerializeField] public Toggle FEToggle;
    [SerializeField] public Toggle SYSToggle;

    [SerializeField] public Text MessageLog;
    
    private string _CreateSession = "EPOCX-E50205EB";
    private string _StartRecord = "unity_1023_";
    private string _StopRecord = "unity_end_1023_";
    private string _LoadPrifile = "Doyo";

    void Start()
    {
        // init EmotivUnityItf without data buffer using
        _eItf.Init(_clientId, _clientSecret, _appName, _appVersion, _isDataBufferUsing);

        // Start
        _eItf.Start();

        //sm
        StartCoroutine(DelayedExecution());

    }

    IEnumerator DelayedExecution()
    {
        yield return new WaitForSeconds(3f);  // 2초 대기
        onCreateSessionBtnClick(); // 헤드셋 입력
        yield return new WaitForSeconds(4f);  // 2초 대기
        onSubscribeBtnClick(); // pow 섭스크라이브

        yield return new WaitForSeconds(2f);  // 1초 대기
        onStartCoherenceBtnClick(); // 코히어런스 계ㅅ
        yield return new WaitForSeconds(2f);  // 1초 대기
        onLoadProfileBtnClick(); // 프로필 로드
    }
    

    // Update is called once per frame
    void Update()
    {
        _timerDataUpdate += Time.deltaTime;
        if (_timerDataUpdate < TIME_UPDATE_DATA) 
            return;

        _timerDataUpdate -= TIME_UPDATE_DATA;
        // update message log
        MessageLog.text = _eItf.MessageLog;
        
        if (!_eItf.IsAuthorizedOK)
            return;

        // Check buttons interactable
        CheckButtonsInteractable();

        // If save data to Data buffer. You can do the same EEG to get other data streams
        // Otherwise check output data at OnEEGDataReceived(), OnMotionDataReceived() ..etc..
        if (_isDataBufferUsing) {
            // get eeg data
            if (_eItf.GetNumberEEGSamples() > 0) {
                string eegHeaderStr = "EEG Header: ";
                string eegDataStr   = "EEG Data: ";
                foreach (var ele in _eItf.GetEEGChannels()) {
                    string chanStr  = ChannelStringList.ChannelToString(ele);
                    double[] data     = _eItf.GetEEGData(ele);
                    eegHeaderStr    += chanStr + ", ";
                    if (data != null && data.Length > 0)
                        eegDataStr      +=  data[0].ToString() + ", ";
                    else
                        eegDataStr      +=  "null, "; // for null value
                }
                string msgLog = eegHeaderStr + "\n" + eegDataStr;
                MessageLog.text = msgLog;
            }
        }

    }

    /// <summary>
    /// create session 
    /// </summary>
    public void onCreateSessionBtnClick() {
        Debug.Log("onCreateSessionBtnClick");
        if (!_eItf.IsSessionCreated)
        {
            //_eItf.CreateSessionWithHeadset(HeadsetId.text);
            _eItf.CreateSessionWithHeadset(_CreateSession);
        }
        else
        {
            UnityEngine.Debug.LogError("There is a session created.");
        }
    }

    /// <summary>
    /// start a record 
    /// </summary>
    public void onStartRecordBtnClick() {
        Debug.Log("onStartRecordBtnClick " + RecordTitle.text + ":" + RecordDescription.text);
        if (_eItf.IsSessionCreated && !string.IsNullOrEmpty(RecordTitle.text))
        {
            //_eItf.StartRecord(RecordTitle.text, RecordDescription.text);
            _eItf.StartRecord(_StartRecord, _StopRecord);
        }
        else {
            UnityEngine.Debug.LogError("Can not start a record because there is no active session or record title is empty.");
        }
    }

    /// <summary>
    /// start a record 
    /// </summary>
    public void onStopRecordBtnClick() {
        Debug.Log("onStopRecordBtnClick");
        _eItf.StopRecord();
    }

    /// <summary>
    /// inject marker
    /// </summary>
    public void onInjectMarkerBtnClick() {
        Debug.Log("onInjectMarkerBtnClick " + MarkerValue.text + ":" + MarkerLabel.text);
        _eItf.InjectMarker(MarkerLabel.text, MarkerLabel.text);
    }

    /// <summary>
    /// subscribe data stream
    /// </summary>
    public void onSubscribeBtnClick() {
        Debug.Log("onSubscribeBtnClick: " + _eItf.IsSessionCreated + ": " + GetStreamsList().Count);
        if (_eItf.IsSessionCreated)
        {
            if (GetStreamsList().Count == 0) {
                UnityEngine.Debug.LogError("The stream name is empty. Please set a valid stream name before subscribing.");
            }
            else {
                _eItf.DataSubLog = ""; // clear data subscribing log
                _eItf.SubscribeData(GetStreamsList());
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Must create a session first before subscribing data.");
        }
    }

    /// <summary>
    /// un-subscribe data
    /// </summary>
    public void onUnsubscribeBtnClick() {
        Debug.Log("onUnsubscribeBtnClick");
        if (GetStreamsList().Count == 0) {
            UnityEngine.Debug.LogError("The stream name is empty. Please set a valid stream name before unsubscribing.");
        }
        else {
            _eItf.DataSubLog = ""; // clear data subscribing log
            _eItf.UnSubscribeData(GetStreamsList());
        }
    }

    /// <summary>
    /// load an exited profile or create a new profile then load the profile
    /// </summary>
    public void onLoadProfileBtnClick() {
        Debug.Log("onLoadProfileBtnClick " + _LoadPrifile);
        _eItf.LoadProfile(_LoadPrifile);
        //Debug.Log("onLoadProfileBtnClick " + ProfileName.text);
        //_eItf.LoadProfile(ProfileName.text);
    }

    public GameObject coherenceScript;  // 코히어런스 스크립트의 참조를 저장
    //sm 코히어런스
    public void onStartCoherenceBtnClick()
    {
        Debug.Log("onStartCoherenceBtnClick");
        coherenceScript.SetActive(true);  // 코히어런스 스크립트 활성화
    }

    /// <summary>
    /// unload a profile
    /// </summary>
    public void onUnLoadProfileBtnClick() {
        Debug.Log("onUnLoadProfileBtnClick " + _LoadPrifile);
        _eItf.UnLoadProfile(_LoadPrifile);
        //Debug.Log("onUnLoadProfileBtnClick " + ProfileName.text);
        //_eItf.UnLoadProfile(ProfileName.text);
    }

    /// <summary>
    /// save a profile
    /// </summary>
    public void onSaveProfileBtnClick() {
        Debug.Log("onSaveProfileBtnClick " + _LoadPrifile);
        _eItf.SaveProfile(_LoadPrifile);
        //Debug.Log("onSaveProfileBtnClick " + ProfileName.text);
        //_eItf.SaveProfile(ProfileName.text);
    }

    /// <summary>
    /// start a mental command training action
    /// </summary>
    public void onStartMCTrainingBtnClick() {
        if (_eItf.IsProfileLoaded)
            _eItf.StartMCTraining(ActionNameList.captionText.text);
        else
            UnityEngine.Debug.LogError("onStartMCTrainingBtnClick: Please load a profile before starting training.");
    }

    /// <summary>
    /// accept a mental command training
    /// </summary>
    public void onAcceptMCTrainingBtnClick() {
        if (_eItf.IsProfileLoaded)
            _eItf.AcceptMCTraining();
        else
            UnityEngine.Debug.LogError("onAcceptMCTrainingBtnClick: Please load a profile before start training.");
    }

    /// <summary>
    /// reject a mental command training
    /// </summary>
    public void onRejectMCTrainingBtnClick() {
        if (_eItf.IsProfileLoaded)
            _eItf.RejectMCTraining();
        else
            UnityEngine.Debug.LogError("onRejectMCTrainingBtnClick: Please load a profile before start training.");
    }

    /// <summary>
    /// erase a mental command training
    /// </summary>
    public void onEraseMCTrainingBtnClick() {
        Debug.Log("onEraseMCTrainingBtnClick " + ActionNameList.captionText.text);
        if (_eItf.IsProfileLoaded)
            _eItf.EraseMCTraining(ActionNameList.captionText.text);
        else
            UnityEngine.Debug.LogError("onAcceptMCTrainingBtnClick: Please load a profile before start training.");
    }


    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        _eItf.Stop();
    }

    private void CheckButtonsInteractable()
    {
        if (!_eItf.IsAuthorizedOK)
            return;

        Button createSessionBtn = GameObject.Find("SessionPart").transform.Find("createSessionBtn").GetComponent<Button>();
        if (!createSessionBtn.interactable)
        {
            createSessionBtn.interactable = true;
            return;
        }


        if (!_eItf.IsSessionCreated)
            return;
        
        // make startRecordBtn interactable
        Button startRecordBtn = GameObject.Find("RecordPart").transform.Find("startRecordBtn").GetComponent<Button>();
        Button subscribeBtn = GameObject.Find("SubscribeDataPart").transform.Find("subscribeBtn").GetComponent<Button>();
        Button unsubscribeBtn = GameObject.Find("SubscribeDataPart").transform.Find("unsubscribeBtn").GetComponent<Button>();
        Button loadProfileBtn = GameObject.Find("TrainingPart").transform.Find("loadProfileBtn").GetComponent<Button>();
        Button unloadProfileBtn = GameObject.Find("TrainingPart").transform.Find("unloadProfileBtn").GetComponent<Button>();
        Button saveProfileBtn = GameObject.Find("TrainingPart").transform.Find("saveProfileBtn").GetComponent<Button>();
        Button rejectTrainingBtn = GameObject.Find("TrainingPart").transform.Find("rejectTrainingBtn").GetComponent<Button>();
        Button eraseTrainingBtn = GameObject.Find("TrainingPart").transform.Find("eraseTrainingBtn").GetComponent<Button>();
        Button acceptTrainingBtn = GameObject.Find("TrainingPart").transform.Find("acceptTrainingBtn").GetComponent<Button>();
        Button startTrainingBtn = GameObject.Find("TrainingPart").transform.Find("startTrainingBtn").GetComponent<Button>();
        Button stopRecordBtn = GameObject.Find("RecordPart").transform.Find("stopRecordBtn").GetComponent<Button>();
        Button injectMarkerBtn = GameObject.Find("RecordPart").transform.Find("injectMarkerBtn").GetComponent<Button>();

        if (!startRecordBtn.interactable)
        {
            startRecordBtn.interactable = true;
        }
        if (!subscribeBtn.interactable)
        {
            subscribeBtn.interactable = true;
        }
        if (!unsubscribeBtn.interactable)
        {
            unsubscribeBtn.interactable = true;
        }
        if (!loadProfileBtn.interactable)
        {
            loadProfileBtn.interactable = true;
        }

        saveProfileBtn.interactable = _eItf.IsProfileLoaded;
        startTrainingBtn.interactable = _eItf.IsProfileLoaded;
        rejectTrainingBtn.interactable = _eItf.IsProfileLoaded;
        eraseTrainingBtn.interactable = _eItf.IsProfileLoaded;
        acceptTrainingBtn.interactable = _eItf.IsProfileLoaded;
        unloadProfileBtn.interactable = _eItf.IsProfileLoaded;

        stopRecordBtn.interactable = _eItf.IsRecording;
        injectMarkerBtn.interactable = _eItf.IsRecording;
    }

    private List<string> GetStreamsList() {
        List<string> _streams = new List<string> {};
            _streams.Add("pow");
        return _streams;
    }
    //private List<string> GetStreamsList()
    //{
    //    List<string> _streams = new List<string> { };
    //    if (EEGToggle.isOn)
    //    {
    //        _streams.Add("eeg");
    //    }
    //    if (MOTToggle.isOn)
    //    {
    //        _streams.Add("mot");
    //    }
    //    if (PMToggle.isOn)
    //    {
    //        _streams.Add("met");
    //    }
    //    if (CQToggle.isOn)
    //    {
    //        _streams.Add("dev");
    //    }
    //    if (SYSToggle.isOn)
    //    {
    //        _streams.Add("sys");
    //    }
    //    if (EQToggle.isOn)
    //    {
    //        _streams.Add("eq");
    //    }
    //    if (POWToggle.isOn)
    //    {
    //        _streams.Add("pow");
    //    }
    //    if (FEToggle.isOn)
    //    {
    //        _streams.Add("fac");
    //    }
    //    if (COMToggle.isOn)
    //    {
    //        _streams.Add("com");
    //    }
    //    return _streams;
    //}
}
