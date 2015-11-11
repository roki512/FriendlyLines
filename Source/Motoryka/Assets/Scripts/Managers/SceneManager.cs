﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using LineManagement;
using LineManagement.GLLines;

public class SceneManager : BaseLvlManager<SceneManager>
{
    ShapeGenerator sGen;
    ILine userLine;
    PathAnalyser analizer;

    public InputHandler inputHandler;

    public LineDrawer lineDrawer;

    public float collapsingTime = 0.3f;

    bool drewThisRound = false;

    public override void Init()
    {
        if (sGen != null)
            sGen = GetComponent<ShapeGenerator>();
        else
        {
            sGen = gameObject.AddComponent<ShapeGenerator>();
        }
        analizer = new PathAnalyser();
    }

    // Use this for initialization
    protected override void PreStart()
    {
        if (inputHandler == null)
            inputHandler = GameObject.FindObjectOfType<InputHandler>();

        if (inputHandler != null)
        {
            inputHandler.press += OnStartDraw;
            inputHandler.release += OnStopDraw;
            inputHandler.move += OnMove;
        }

        if (this.lineDrawer == null)
        {
            this.lineDrawer = GameObject.FindObjectOfType<LineDrawer>();
        }

        ShapeElement prevVertices = GameManager.Instance.GetPreviousShapeVertices();

        this.sGen.color = GameManager.Instance._config.Levels[GameManager.Instance.CurrentLevel - 1].shapeColor;
        this.sGen.size = GameManager.Instance._config.Levels[GameManager.Instance.CurrentLevel - 1].shapeStroke;

        this.lineDrawer.color = GameManager.Instance._config.Levels[GameManager.Instance.CurrentLevel - 1].brushColor;
        this.lineDrawer.size = GameManager.Instance._config.Levels[GameManager.Instance.CurrentLevel - 1].brushStroke;

		this.sGen.drawStartPoint = GameManager.Instance._config.DrawStartPoint;

        if (prevVertices == null)
        {
            shape = sGen.CreateShape(GameManager.Instance.GetCurrentShape());
        }
        else
        {

            shape = prevVertices;//sGen.CreateShape(prevVertices);
        }

        drewThisRound = false;

        //this.shape = this.sGen.CreateShape(this.sGen.CollapseShape(shape));
    }

    protected override void PreFinish()
    {
        Debug.Log("Prefinish");

        lineDrawer.StopDrawing();

        shape.Shape.CollapseToPoint(Vector2.zero, collapsingTime);
        shape.StartPoint.CollapseToPoint(Vector2.zero, collapsingTime);
        userLine.CollapseToPoint(Vector2.zero, collapsingTime);

        StartCoroutine(FinishAfterTime(collapsingTime));
    }

    IEnumerator FinishAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        CurrentPhase = LevelPhase.Finished;
    }

    protected override void PostFinish()
    {

		Debug.Log ("Wynik: " + analizer.GetResult (shape.Shape, userLine) + " %");
        userLine.Delete();
        Animator animator = GetComponent<Animator>();
        string sciezka = Application.dataPath;
        sciezka += "/boredFishController.controller";
        //animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(sciezka);
//        animator.runtimeAnimatorController = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiat(Resources.Load("boredFishController")); // Resources.Load(sciezka) as RuntimeAnimatorController;
        animator.SetTrigger("finished");
        Debug.Log("Animation trigger is set");
    }



    public void RegisterUserLine(ILine line)
    {
        userLine = line;
    }

    public bool IsFinished()
    {
        if (userLine != null)
            return analizer.IsFinished(shape.Shape, userLine);
        return false;
    }

	public bool IsStartCorrect(Vector3 where) 
	{
		return analizer.IsStartCorrect (where, shape.Shape);
	}

    public void OnStopDraw()
    {
        if (drewThisRound)
		{
			Debug.Log ("Wynik: " + analizer.GetResult (shape.Shape, userLine) + " %");
            if (IsFinished())
            {
                CurrentPhase = LevelPhase.Prefinished;
            }
            else
            {
                RestartLevel();
            }
        }
    }

    public void OnMove(Vector3 pos)
    {
        if (inputHandler.lineDrawer.IsDrawing && IsFinished())
        {
            CurrentPhase = LevelPhase.Prefinished;
        }
    }

    public void OnStartDraw(Vector3 pos)
    {
        drewThisRound = true;
        RegisterUserLine(inputHandler.lineDrawer.CurrentLine);
    }

}
