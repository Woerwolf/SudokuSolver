﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CleanSolver : MonoBehaviour
{
    public List<Solver> solvers = new List<Solver>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Solver s in solvers)
        {
            s.Update();
        }
    }
}

public class Solver 
{
    bool instantSolve;
    float timeNeeded;
    int frames = 0;
    long steps;
    int repetitionRate;
    float startingTime;
    private tile active;
    public field field;
    private bool backtrackingRunning;

    public Solver(field fieldToSolve, int frameRepetitionRate = 1) {
        repetitionRate = frameRepetitionRate;
        field = fieldToSolve;
        field.GetPossibleValues();
        backtrackingRunning = false;
        GameObject.Find("CleanSolver").GetComponent<CleanSolver>().solvers.Add(this);
    }

    public void Update()
    {
        frames++;
        // check if backtracking should run, then check if it returns true. 
        // If so, it has finished and "backtrackingRunning can be set to "false".
        repetitionRate = Stats.speed;
        for(int i = 0; i<repetitionRate; i++)
        {
            if (backtrackingRunning && BacktrackingStep()) StopBacktracking();
        }


        // slow version for debugging
        //if(frames%repetitionRate == 0)
        //{
        //    if (backtrackingRunning && BacktrackingStep()) StopBacktracking();
        //}
    }

    public void Backtracking() {
        instantSolve = (GameObject.Find("SliderSpeed").GetComponent<Slider>().value.Equals(GameObject.Find("SliderSpeed").GetComponent<Slider>().maxValue));
        backtrackingRunning = true;
        startingTime = Time.realtimeSinceStartup;
        SetTrivialValues();
        if (field.firstEmpty != null) active = field.firstEmpty;
    }

    public void StopBacktracking() {
        backtrackingRunning = false;
        //GameObject.Find("Solve").GetComponentInChildren<Text>().text = "Solve Sudoku";
    }

    public bool BacktrackingStep() {
        //Debug.Log(active.row + "  " + active.column);
        //timeNeeded = Time.realtimeSinceStartup - startingTime;
        //Debug.Log("Timer 1: " + timeNeeded + "s!");
        //startingTime = Time.realtimeSinceStartup;
        if (field.GetAmountOfMissingValues() == 0)
        {
            field.Print();
            if (field.IsValid())
            {
                timeNeeded = Time.realtimeSinceStartup - startingTime;
                Debug.Log("Sudoku Solved in " + timeNeeded + "s!");
                return true;
            }
            Debug.LogError("Sudoku can not be solved!");
            backtrackingRunning = false;
            return false;
        }

        //timeNeeded = Time.realtimeSinceStartup - startingTime;
        //Debug.Log("Timer 2: " + timeNeeded + "s!");
        //startingTime = Time.realtimeSinceStartup;

        bool nextPossibilityAvailable = active.TestNextPossibility();
        bool valid = field.IsValidForOneTile(active.row, active.column, false);
        //Debug.Log(valid + "   " + active.value);
        if (nextPossibilityAvailable && valid) {
            if (active.nextEmpty != null)
            {
                active = active.nextEmpty;
                field.GetPossibleValuesForOneTile(active.row, active.column);
                if(!instantSolve)field.Print();
                //timeNeeded = Time.realtimeSinceStartup - startingTime;
                //Debug.Log("Timer 3: " + timeNeeded + "s!");
                //startingTime = Time.realtimeSinceStartup;
                return false;
            }
            else
            {
                field.Print();
                timeNeeded = Time.realtimeSinceStartup - startingTime;
                Debug.Log("Sudoku Solved in " + timeNeeded + "s!");
                //timeNeeded = Time.realtimeSinceStartup - startingTime;
                //Debug.Log("Timer 4: " + timeNeeded + "s!");
                //startingTime = Time.realtimeSinceStartup;
                return true;
            }
        }
        else
        {
            //if (!nextPossibilityAvailable)
            //{
                if(active.previousEmpty == null)
                {
                    Debug.LogError("No Solution Found!");
                    backtrackingRunning = false;
                    return false;
                }
                active.value = 0;
                active = active.previousEmpty;
            //}
        }
        if(!instantSolve) field.Print();
        //timeNeeded = Time.realtimeSinceStartup - startingTime;
        //Debug.Log("Timer 5: " + timeNeeded + "s!");
        //startingTime = Time.realtimeSinceStartup;
        return false;
    }

    public void SetTrivialValues() {
        tile activeTile = field.firstEmpty;
        do
        {
            if (activeTile.amountOfPossibilities == 1)
            {
                activeTile.TestNextPossibility();
            }
            activeTile = activeTile.nextEmpty;
        } while (activeTile.nextEmpty != null);
        field.GetPossibleValues();
        field.UpdateEmptyDLL();
    }
}

public class field 
{
    public tile[,] tiles;
    public tile firstEmpty;

    public field(int[,] values)
    {
        tiles = new tile[values.GetLength(0), values.GetLength(1)];
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                // Do this for all tiles in field
                // Give tiles values
                tiles[i, j] = new tile(values[i, j], i, j);
            }
        }
        UpdateEmptyDLL();
    }

    // relink double linked list containing all empty tiles (value = 0)
    public void UpdateEmptyDLL() {
        tile emptyTemp = null;
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                // get predecessor and successor
                if (tiles[i, j].value == 0)
                {
                    if (emptyTemp != null)
                    {
                        emptyTemp.nextEmpty = tiles[i, j];
                        tiles[i, j].previousEmpty = emptyTemp;
                    }
                    else
                    {
                        firstEmpty = tiles[i, j];
                    }
                    emptyTemp = tiles[i, j];
                }
            }
        }
    }

    public void GetPossibleValues()
    {
        bool doItAgain = false;
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                // Do this for all tiles in field
                GetPossibleValuesForOneTile(i, j);
            }
        }

        //if (doItAgain)
        //{
        //    Debug.Log("AGAIN");
        //    GetPossibleValues();
        //    return;
        //}
    }

    // returns amount of possibilities
    public int GetPossibleValuesForOneTile(int row, int column)
    {
        // at beginning, all values are still possible
        bool[] valuesTemp = new bool[9];
        for (int k = 0; k < valuesTemp.Length; k++)
        {
            valuesTemp[k] = true;
        }
        // go through all relevant tiles and sort out none possible values
        for (int k = 0; k < tiles.GetLength(0); k++)
        {
            if (tiles[k, column].value > 0)
            {
                valuesTemp[tiles[k, column].value - 1] = false;
            }
        }
        for (int k = 0; k < tiles.GetLength(1); k++)
        {
            if (tiles[row, k].value > 0)
            {
                valuesTemp[tiles[row, k].value - 1] = false;
            }
        }
        for (int k = row - (row % 3); k < row - (row % 3) + 3; k++)
        {
            for (int l = column - (column % 3); l < column - (column % 3) + 3; l++)
            {
                if (tiles[k, l].value > 0)
                {
                    valuesTemp[tiles[k, l].value - 1] = false;
                }
            }
        }

        tiles[row, column].possibleValues = valuesTemp;

        int cnt = 0;
        for(int i = 0; i<valuesTemp.Length; i++)
        {
            if (valuesTemp[i]) cnt++;
        }

        tiles[row, column].amountOfPossibilities = cnt;

        // if only one possibility exists, paste this one to "value"
        //if(cnt == 1)
        //{
        //    tiles[row, column].TestNextPossibility();
        //    tiles[row, column].possibleValues = new bool[9];
        //    UpdateEmptyDLL();
        //}

        return cnt;
    }

    public void Print()
    {
        int[,] values = new int[9, 9];
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                values[i, j] = tiles[i, j].value;
            }
        }
        GameObject.Find("FieldSpawner").GetComponent<FieldSpawner>().PrintSolution(values);
    }

    public bool IsValid() {
        //if an error is found, "false" gets returned, else return "true" at the end of method

        // control all rows
        bool[] foundValues = new bool[tiles.GetLength(0)];
        for(int i = 0; i<tiles.GetLength(0); i++)
        {
            // erase foundValues
            for (int j = 0; j < foundValues.Length; j++)
            {
                foundValues[j] = false;
            }
            // control rows
            for (int j = 0; j<tiles.GetLength(1); j++)
            {
                if (tiles[i, j].value > 0)
                {
                    if (foundValues[tiles[i, j].value - 1]) return false;
                    foundValues[tiles[i, j].value - 1] = true;
                }
            }
        }
        for (int i = 0; i < tiles.GetLength(1); i++)
        {
            // erase foundValues
            for (int j = 0; j < foundValues.Length; j++)
            {
                foundValues[j] = false;
            }
            // control columns
            for (int j = 0; j < tiles.GetLength(0); j++)
            {
                if (tiles[j, i].value > 0)
                {
                    if (foundValues[tiles[j, i].value - 1]) return false;
                    foundValues[tiles[j, i].value - 1] = true;
                }
            }
        }
        for(int i = 0; i<tiles.GetLength(0); i += 3)
        {
            for (int j = 0; j < tiles.GetLength(1); j += 3) {
                // erase foundValues
                for (int k = 0; k < foundValues.Length; k++)
                {
                    foundValues[k] = false;
                }
                // control small fields
                for(int l = 0; l<3; l++)
                {
                    for(int m = 0; m<3; m++)
                    {
                        if (tiles[i+l, j+m].value > 0)
                        {
                            if (foundValues[tiles[i + l, j + m].value - 1]) return false;
                            foundValues[tiles[i + l, j + m].value - 1] = true;
                        }
                    }
                }
            }
        }
        return true;
    }

    public bool IsValidForOneTile(int row, int column, bool ignoreZeros = false) {
        int value = tiles[row, column].value;
        if (value == 0) return ignoreZeros;
        for(int i = 0; i<9; i++) {
            if (tiles[row, i].value == value && i != column) return false;
            if (tiles[i, column].value == value && i != row) return false;
            if (tiles[row-(row%3) + i / 3, column-(column%3) + (i % 3)].value == value && i != (row % 3) * 3 + (column % 3)) return false;
        }
        return true;
    }

    public int GetAmountOfMissingValues() {
        if (firstEmpty == null) return 0;
        int cnt = 1;
        tile active = firstEmpty;
        do
        {
            active = active.nextEmpty;
            cnt++;
        } while (active.nextEmpty != null);
        return cnt;
    }
}

public class tile
{
    public int row, column, value, initialValue, amountOfPossibilities;
    public tile previousEmpty, nextEmpty;
    public bool[] possibleValues;
    public tile(int value, int row, int column) {
        this.value = value;
        this.initialValue = value;
        this.row = row;
        this.column = column;
        amountOfPossibilities = 0;
    }

    // return value is an Indicator, if the method worked correctly
    public bool TestNextPossibility() { 
        if(possibleValues == null || value>=possibleValues.Length) return false;
        for(int i = value; i<possibleValues.Length; i++)
        {
            if (possibleValues[i])
            {
                value = i + 1;
                return true;
            }
        }
        return false;
    }
}

