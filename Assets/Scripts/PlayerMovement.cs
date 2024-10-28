using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFramework;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    Vector2 cursorReleasePos;
    Rigidbody2D rb;
    bool gameOver, reachedNextLevel;
    [SerializeField]
    Transform lavaTrans, miniMapCamTrans;

    [SerializeField]
    float forceMul, maxExplodeForce, 
    minEnergyNeed, energyNeedMul,
    initialEnergyRefuel, energyNeedNormal,
    energyNeedSlowmoMul, initialEnergy, 
    energyRemaining;

    [SerializeField]
    float maxEnergyIncreasePerLv, energyRefuelIncreasePerLv, energyWasteReducePercentagePerLv;

    float energyWastePercentage, maxEnergy, energyRefuel;

    [SerializeField]
    EnergyGenerator generator;

    [SerializeField][Range(0, 1)]
    float timeScale;

    [SerializeField]
    Image slider, heightSlider;

    [SerializeField]
    GameObject gameOverPanel;

    [SerializeField]
    TMPro.TMP_Text heightText, goldCountText;

    int goldCount;

    bool alreadyPaused;
    
    [SerializeField]    List<GameObject> dots;

    // Start is called before the first frame update
    void Start()
    {
        reachedNextLevel = false;
        gameOver = false;
        goldCount = 0;
        alreadyPaused = false;
        maxEnergy = GameManager.instance.maxEnergyLv*maxEnergyIncreasePerLv+initialEnergy;
        energyRemaining = maxEnergy;
        energyRefuel = initialEnergyRefuel + GameManager.instance.energyRefuelLv*energyRefuelIncreasePerLv;
        energyWastePercentage = (100-GameManager.instance.energyWasteLv*energyWasteReducePercentagePerLv)/100;
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;
        rb = GetComponent<Rigidbody2D>();
        gameOverPanel.SetActive(false);
        foreach (var Go in dots){
            Go.SetActive(false);

        }
    }

    Vector2 velocityBeforePause;
    // Update is called once per frame
    void Update()
    {
        miniMapCamTrans.position = new Vector3(transform.position.x, transform.position.y, miniMapCamTrans.position.z);
        if (gameOver) return;
        if (GameManager.instance.gamePaused){
            // if (alreadyPaused) {
            //     rb.linearVelocity = new Vector2(0, 0);
            // }
            // else{
            //     velocityBeforePause = rb.linearVelocity;
            //     alreadyPaused = true;
            //     rb.linearVelocity = new Vector2(0, 0);
            //     Time.timeScale = 1;
            // }
            if (!alreadyPaused){
                alreadyPaused = true;
            }
            Time.timeScale = 0;
            return;
        }
        else{
            if (alreadyPaused){
                // rb.linearVelocity = velocityBeforePause;
                Time.timeScale = 1;
                alreadyPaused = false;
            }
            UpdateLava();
            energyRemaining -= Time.deltaTime/(1/energyNeedNormal);
            heightSlider.fillAmount = transform.position.y / 1000;
            heightText.rectTransform.localPosition = new Vector3(heightText.rectTransform.localPosition.x, heightSlider.fillAmount * 600 - 300, heightText.rectTransform.localPosition.z);
            heightText.text = ((int)transform.position.y).ToString();
            GameManager.instance.inGameCoinCnt = goldCount;
            slider.fillAmount = energyRemaining/maxEnergy;

            // gameover
            if (transform.position.y <= lavaTrans.position.y + 15.5 & !gameOver){
                gameOverPanel.SetActive(true);
                GameManager.instance.coinCount += goldCount*GameManager.instance.gameLevel;
                GameManager.instance.SaveGame();
                // rb.bodyType = RigidbodyType2D.Static;
                Time.timeScale = 0;
                gameOver = true;
                return;
            }

            if (energyRemaining <= 0){
                energyRemaining = 0;
                return;
            }
            if (transform.position.y > 1000 && !reachedNextLevel){
                GameManager.instance.gameLevel += 1;
                reachedNextLevel = true;
            }
            else if (energyRemaining > maxEnergy){
                energyRemaining = maxEnergy;
            }
            if(Input.GetMouseButtonDown(0)){
                Time.timeScale = timeScale;
                Time.fixedDeltaTime = 0.02f * timeScale;
                foreach (var dot in dots){
                    dot.SetActive(true);
                }
            }
            if(Input.GetMouseButton(0)){
                OnCursorPress();
                energyRemaining -= Time.deltaTime/(1/(energyNeedNormal*energyNeedSlowmoMul));
            }
            if(Input.GetMouseButtonUp(0)){
                OnCursorRelease();
                Time.timeScale = 1;
                Time.fixedDeltaTime = 0.02f;
                foreach (var dot in dots){
                    dot.SetActive(false);
                }
            }
        }
    }
    void OnCursorPress(){
        cursorReleasePos += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        // Debug.Log("Cursor pressed at : " + cursorReleasePos);
        ShowProjectile();
    }

    void UpdateLava(){
        if ((transform.position.y - lavaTrans.position.y) < 115){
            lavaTrans.position = new Vector2(transform.position.x, lavaTrans.position.y);
        }
        else{
            lavaTrans.position = new Vector2(transform.position.x, transform.position.y-115);
        }
    }

    void ShowProjectile(){
        var velocity = new Vector2();
        var CurrentVelocity = rb.linearVelocity/2;
        var energyNeed = minEnergyNeed+(energyNeedMul * cursorReleasePos.magnitude);
        var direction = (-cursorReleasePos).normalized;
        if(energyNeed > energyRemaining){
            velocity = CurrentVelocity + direction*energyRemaining*forceMul/50;
        }
        else{
            velocity = CurrentVelocity + direction*energyNeed*forceMul/50;
        }
        float t = 0.0f;
        foreach(var dot in dots){
            var x = velocity.x*t+transform.position.x;
            var y = velocity.y*t+0.5f*(-9.81f)*t*t+transform.position.y;
            // Debug.Log("velocity: "+velocity+" x: "+x+" y: "+y);
            dot.transform.position = new Vector3(x, y);
            t += 0.2f;
        }
    }

    void OnCursorRelease(){
        rb.linearVelocity = rb.linearVelocity/2;
        var energyNeed = minEnergyNeed+(energyNeedMul * cursorReleasePos.magnitude);
        var direction = (-cursorReleasePos).normalized;
        if(energyNeed > energyRemaining){
            rb.AddForce(direction*energyRemaining*forceMul);
            energyRemaining = 0;
            cursorReleasePos = Vector2.zero;
            return;
        }
        rb.AddForce(direction*energyNeed*forceMul);
        // Debug.Log("Cursor released at : " + cursorReleasePos);
        energyRemaining -= energyNeed*energyWastePercentage;
        cursorReleasePos = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Energy"){
            var explodeForce = new Vector2(Random.Range(-maxExplodeForce, maxExplodeForce), Random.Range(-maxExplodeForce/5, maxExplodeForce));
            rb.AddForce(explodeForce, ForceMode2D.Impulse);
            generator.RemoveObject(other.transform);
            energyRemaining += energyRefuel;
        }
        if (other.gameObject.tag == "Bomb"){
            var explodeForce = new Vector2(Random.Range(-maxExplodeForce, maxExplodeForce), Random.Range(-maxExplodeForce/5, maxExplodeForce));
            rb.AddForce(explodeForce, ForceMode2D.Impulse);
            generator.RemoveObject(other.transform);
            energyRemaining -= 30;
        }
    }

    // TODO: Find out should coin provide energy
    private void OnTriggerEnter2D(Collider2D other) {
        // Debug.Log("On trigger enter");
        if (other.gameObject.tag == "Coin"){
            energyRemaining += energyRefuel/2;
            goldCount++;
            generator.RemoveObject(other.transform);
            goldCountText.text = goldCount.ToString();
        }
    }
}
