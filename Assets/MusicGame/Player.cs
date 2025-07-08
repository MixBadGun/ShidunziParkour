using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputStruct;
using TouchPhase = UnityEngine.TouchPhase;

public class Player : MonoBehaviour
{
    public static Player targetPlayer;

    public GameObject center;
    private Vector3 velocity;
    public int now_track = 2;
    public BeatmapManager beatmapManager;
    public const int MAX_TRACKS = 3;
    private float all_timer = 0;
    private float loosen_time = 0;
    private bool toMoving = false;
    private bool isMoving = false;
    private bool isDrop = false;
    public float gravity = 50f;
    public const int TRACK_WIDTH = 3;
    private float origin_pos = 0;
    private float should_pos = 0;
    private float delta_pos = 0;
    private float move_timer = 0f; // 计时器
    public const float MAX_CROSS_TIME = 0.2125f;
    public float cross_time = MAX_CROSS_TIME;
    private bool isFlying = false;
    private List<FromTo> movementList = new();

    public Animator[] TouchAnimators;
    public GameObject PreventTouchBox;

    public List<InputImpluse> inputImpluses = new();

    private XinputControls xinputActions;

    void Awake()
    {
        xinputActions = new XinputControls();
        if (Gamepad.current != null && DataStorager.settings.useGamepad)
        {
            xinputActions.Enable();
        }

        float speed = DataStorager.settings.MusicGameSpeed > 0 ? DataStorager.settings.MusicGameSpeed : 1;
        velocity.z = 50 * speed;
        targetPlayer = this;
    }

    private void OnDestroy()
    {
        xinputActions.Disable();
    }

    void CreateNewInputImpluse(int num) {
        inputImpluses.Add(
            new InputImpluse(){
                track = num,
                time = Time.fixedTime
            }
        );
    }

    void inputUpdate() {
        while(inputImpluses.Count > 0){
            if(Time.fixedTime - inputImpluses[0].time <= 0.1){
                break;
            }
            inputImpluses.RemoveAt(0);
        }
    }

    IEnumerator FixPos(){
        while (true)
        {
            ChangePos();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ChangePos()
    {
        Vector3 pos = transform.position;
        pos.z = beatmapManager.GetPlayingTime() * velocity.z;
        transform.position = pos;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FixPos());
        updateGravity();
    }

    void updateGravity(){
        gravity = (float)(2 * 8 / Math.Pow(60 / beatmapManager.getBPM(),2));
    }

    public float GetGravity(){
        return gravity;
    }

    // Update is called once per frame
    void FixedUpdate(){
        all_timer += Time.fixedDeltaTime;

        inputUpdate();
        updatePosHorizon();

        // 着地
        if (transform.position.y < 0)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0 , gameObject.transform.position.z);
            velocity.y = 0;
            isFlying = false;
            isDrop = false;
        }
        if(isFlying){
            velocity = new Vector3(velocity.x, velocity.y - gravity * Time.fixedDeltaTime, velocity.z);
        }

        gameObject.transform.position += velocity * Time.fixedDeltaTime;
        center.transform.rotation = Quaternion.Euler(all_timer * velocity.z * 32,0,0);
    }

    private int jump_level = 0;
    private float clear_time = 0.25f;
    void handleStickInput()
    {
        if (!xinputActions.Xinput.enabled)
        {
            return;
        }
        int track = (int)(((xinputActions.Xinput.HorizonMove.ReadValue<float>() + 1f) / 2.0f * MAX_TRACKS) + 1f);
        if (track > MAX_TRACKS)
        {
            track -= 1;
        }
        if (track != now_track)
        {
            moveToIndex(track);
        }

        // 按秒数记录跳跃，每 0.2 一个跳跃级别
        if (clear_time <= 0)
        {
            jump_level = 0;
        }
        else
        {
            clear_time -= Time.deltaTime;
        }
        if (xinputActions.Xinput.VerticalMove.ReadValue<float>() > 0.2 * jump_level)
        {
            moveUp();
            jump_level++;
            clear_time = 0.2f;
        }
        if (xinputActions.Xinput.VerticalMove.ReadValue<float>() < 0)
        {
            moveDown();
        }
    }

    void Update()
    {
        loosen_time += Time.deltaTime;
        handleKeyInput();
        handleFingerInput();
        handleStickInput();
        if (DataStorager.settings.relaxMod)
        {
            handleNumInput();
        }
        updateGravity();

        if(DateTime.Now.Day == 1 && DateTime.Now.Month == 4){
            transform.position = new Vector3(math.cos(Time.time * 40) * 3,1 + math.sin(Time.time * 40),transform.position.z);
        }
    }

    public float GetVelocity()
    {
        return velocity.z;
    }

    public bool isDroping()
    {
        return isDrop;
    }

    public Vector3 GetPos()
    {
        return gameObject.transform.position;
    }


    float CalcOffsetByTimer()
    {
        float t = move_timer += Time.fixedDeltaTime;
        return (float)(delta_pos * (1 - Math.Pow(1 - t / cross_time, 8)));
    }
    void updatePosHorizon()
    {
        if (toMoving)
        {
            origin_pos = gameObject.transform.position.x;
            should_pos = TRACK_WIDTH * (now_track - (MAX_TRACKS + 1) / 2);
            delta_pos = should_pos - gameObject.transform.position.x;
            move_timer = 0f;
            isMoving = true;
        }
        if (isMoving)
        {
            gameObject.transform.position = new Vector3(origin_pos + CalcOffsetByTimer(), gameObject.transform.position.y, gameObject.transform.position.z);
            if (move_timer >= cross_time)
            {
                isMoving = false;
            }
        }
    }

    bool checkGrouned()
    {
        if (transform.position.y < 1)
        {
            isDrop = false;
            return true;
        }
        return false;
    }
    public void moveUp(KeyCode keyCode = KeyCode.None)
    {
        beatmapManager.AddPlayRecord(MoveType.MOVE_UP, keyCode);
        if (checkGrouned() || loosen_time < 0.1)
        {
            // the_rigidbody.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            float new_speed = gravity * 60 / beatmapManager.getBPM();
            velocity += new Vector3(0, new_speed, 0);
            if (!isFlying)
            {
                loosen_time = 0;
            }
            isFlying = true;
            // isGrounded = false;
        }
    }
    public void moveLeft(KeyCode keyCode = KeyCode.None)
    {
        beatmapManager.AddPlayRecord(MoveType.MOVE_LEFT, keyCode);
        if (now_track > 1)
        {
            now_track -= 1;
            CreateNewInputImpluse(now_track);
            toMoving = true;
        }
    }
    public void moveRight(KeyCode keyCode = KeyCode.None)
    {
        beatmapManager.AddPlayRecord(MoveType.MOVE_RIGHT, keyCode);
        if (now_track < MAX_TRACKS)
        {
            now_track += 1;
            CreateNewInputImpluse(now_track);
            toMoving = true;
        }
    }
    public void moveDown(KeyCode keyCode = KeyCode.None)
    {
        beatmapManager.AddPlayRecord(MoveType.MOVE_DOWN, keyCode);
        // the_rigidbody.AddForce(Vector3.down * 50f, ForceMode.Impulse);
        isDrop = true;
        checkGrouned();
        velocity -= new Vector3(0,(float)(gameObject.transform.position.y / 0.025),0);
        CreateNewInputImpluse(now_track);
        gameObject.GetComponent<Animator>().SetTrigger("DownFlat");
    }

    public void setCrossTime(float crotime){
        if(crotime > 0){
            if(crotime > 0.01f){
                cross_time = Math.Min(crotime, MAX_CROSS_TIME);
            } else {
                cross_time = 0.01f;
            }
        } else {
            cross_time = 0.01f;
        }
    }

        public class FromTo
    {
        // Fields
        public int fingerId;
        public Vector2 first;
        public Vector2 second;
    }

    void CalcAndResponse(FromTo fromto)
    {
        Vector2 vec = fromto.second - fromto.first;
        // 距离过小则忽略
        if (Vector2.Distance(Vector2.zero, vec) < 10)
        {
            return;
        }
        int now_index = 0;
        float max_result = Vector2.Dot(vec, Vector2.up);
        if (Vector2.Dot(vec, Vector2.right) > max_result)
        {
            now_index = 1;
            max_result = Vector2.Dot(vec, Vector2.right);
        }
        if (Vector2.Dot(vec, Vector2.down) > max_result)
        {
            now_index = 2;
            max_result = Vector2.Dot(vec, Vector2.down);
        }
        if (Vector2.Dot(vec, Vector2.left) > max_result)
        {
            now_index = 3;
        }
        switch (now_index)
        {
            case 0: moveUp(); break;
            case 1: moveRight(); break;
            case 2: moveDown(); break;
            case 3: moveLeft(); break;
        }
    }

    void FingerBySlide()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                FromTo movement = new()
                {
                    fingerId = touch.fingerId,
                    first = touch.position
                };
                movementList.Add(movement);
            }
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended)
            {

                for(int k = 0; k < movementList.Count; k++){
                    FromTo movement = movementList[k];
                    if(movement.fingerId == touch.fingerId){
                        movement.second = touch.position;
                        if(Vector2.Distance(movement.first,movement.second) > 25 || touch.phase == TouchPhase.Ended){
                            CalcAndResponse(movement);
                            movementList.RemoveAt(k);
                            k--;
                        }
                    }
                }
            }
        }
    }

    enum TouchDirection {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    TouchDirection CalcDirectionByPos(Vector2 pos) {
        Vector2 a = new(0, 0);
        Vector2 b = new(Screen.width, 0);
        Vector2 c = new(Screen.width, Screen.height);
        Vector2 d = new(0, Screen.height);

        bool right = Cross(b - pos, d - pos) > 0;
        bool up = Cross(a - pos, c - pos) > 0;
        if (right)
        {
            if (up)
            {
                return TouchDirection.LEFT;
            }
            return TouchDirection.DOWN;
        }
        if (up)
        {
            return TouchDirection.UP;
        }
        return TouchDirection.RIGHT;
    }

    private float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    bool WithInPreventBox(Vector2 pos) {
        RectTransform rectTrans = PreventTouchBox.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTrans,
            pos,
            null,
            out Vector2 localPos
        );
        return rectTrans.rect.Contains(localPos);
    }

    void FingerByTouch()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                if (WithInPreventBox(touch.position))
                {
                    return;
                }
                switch (CalcDirectionByPos(touch.position))
                {
                    case TouchDirection.UP: TouchAnimators[0].SetTrigger("ArrowTrigger"); moveUp(); break;
                    case TouchDirection.RIGHT: TouchAnimators[1].SetTrigger("ArrowTrigger"); moveRight(); break;
                    case TouchDirection.DOWN: TouchAnimators[2].SetTrigger("ArrowTrigger"); moveDown(); break;
                    case TouchDirection.LEFT: TouchAnimators[3].SetTrigger("ArrowTrigger"); moveLeft(); break;
                }
            }
        }
    }

    void handleFingerInput()
    {
        if (Time.timeScale <= 0)
        {
            return;
        }
        switch (DataStorager.settings.touchControlMode)
        {
            case DataManager.TouchControlMode.TAP: FingerByTouch(); break;
            case DataManager.TouchControlMode.SLIDE: FingerBySlide(); break;
        }
    }

    void handleNumInput(){
        KeyCode[] firstKeys = DataStorager.keysettings.pad1;
        foreach( KeyCode key in firstKeys ){
            if(Input.GetKeyDown(key)){
                 moveToIndex(1,key);
            }
        }

        KeyCode[] secondKeys = DataStorager.keysettings.pad2;
        foreach( KeyCode key in secondKeys ){
            if(Input.GetKeyDown(key)){
                 moveToIndex(2,key);
            }
        }

        KeyCode[] thirdKeys = DataStorager.keysettings.pad3;
        foreach( KeyCode key in thirdKeys ){
            if(Input.GetKeyDown(key)){
                 moveToIndex(3,key);
            }
        }
    }

    public void moveToIndex(int index, KeyCode keyCode = KeyCode.None){
        beatmapManager.AddPlayRecord(MoveType.MOVE_INDEX, keyCode);
        now_track = index;
        CreateNewInputImpluse(now_track);
        toMoving = true;
    }

    void handleKeyInput()
    {
        KeyCode[] leftKeys = DataStorager.keysettings.left;
        foreach (KeyCode key in leftKeys)
        {
            if (Input.GetKeyDown(key))
            {
                moveLeft(key);
            }
        }

        KeyCode[] rightKeys = DataStorager.keysettings.right;
        foreach (KeyCode key in rightKeys)
        {
            if (Input.GetKeyDown(key))
            {
                moveRight(key);
            }
        }

        KeyCode[] upKeys = DataStorager.keysettings.up;
        foreach (KeyCode key in upKeys)
        {
            if (Input.GetKeyDown(key))
            {
                moveUp(key);
            }
        }

        KeyCode[] downKeys = DataStorager.keysettings.down;
        foreach (KeyCode key in downKeys)
        {
            if (Input.GetKeyDown(key))
            {
                moveDown(key);
            }
        }
    }

    public int GetNowTrack(){
        return now_track;
    }
}
