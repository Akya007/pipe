using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PipeSpawner : MonoBehaviour
{
    public int desiredActivePipeCount = 1; // Number of active pipes to maintain
    public float pipeSpeed = 1f; // Speed of the pipes
    public int maxPipesOnScreen = 7; // Maximum number of pipes allowed on screen at once
    public int minPipeTurns = 100; // Minimum number of turns a pipe must make
    public int maxPipeTurns = 200; // Maximum number of turns a pipe can make
    private int activePipes = 0; // Counter for active pipes
    private List<Pipe> finishedPipes = new List<Pipe>(); // List to keep track of finished pipes

    private bool paused = false; // Flag to check if the spawner is paused
    private Pipe currentPipe = null; // Reference to the current pipe being generated

    public Vector3 boundarySize = new Vector3(60, 30, 60); // Boundary size for the pipes

    void Start()
    {
        StartCoroutine(SpawnPipe()); // Start the coroutine to spawn pipes
    }

    IEnumerator SpawnPipe()
    {
        while (!paused)
        {
            if (currentPipe == null)
            {
                Debug.Log("Spawning a new pipe."); // Debugging log
                GameObject pipeObj = new GameObject("Pipe"); // Create a new GameObject for the pipe
                Pipe pipe = pipeObj.AddComponent<Pipe>(); // Add the Pipe component to the GameObject
                pipe.SetSpeed(pipeSpeed); // Set the speed of the pipe
                pipe.SetMaxTurns((int)Random.Range(minPipeTurns, maxPipeTurns)); // Set the maximum number of turns for the pipe
                pipe.pipeColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f); // Set a new random lighter color for the pipe
                pipe.boundarySize = boundarySize; // Set the boundary size for the pipe

                pipe.OnPipeFinished += OnPipeFinished; // Subscribe to the OnPipeFinished event

                activePipes++; // Increment the active pipes count
                currentPipe = pipe; // Set the current pipe
                Debug.Log("Waiting for current pipe to finish."); // Debugging log
            }

            yield return new WaitUntil(() => !currentPipe.isGenerating); // Wait until the current pipe finishes generating
        }
    }

    public bool IsPaused()
    {
        return paused; // Return the paused state
    }

    void SetSpeed(float speed)
    {
        foreach (Pipe pipe in FindObjectsOfType<Pipe>())
        {
            pipe.SetSpeed(speed); // Set the speed for each active pipe
        }
    }

    public void OnPipeFinished(Pipe pipe)
    {
        pipe.OnPipeFinished -= this.OnPipeFinished; // Unsubscribe from the OnPipeFinished event

        activePipes--; // Decrement the active pipes count
        finishedPipes.Add(pipe); // Add the finished pipe to the list
        currentPipe = null; // Clear the reference to the current pipe

        if (activePipes < desiredActivePipeCount)
        {
            Debug.Log("OnPipeFinished: Starting new pipe generation."); // Debugging log
            StartCoroutine(SpawnPipe()); // Spawn new pipes if needed
        }
    }

    public void ResetPipes()
    {
        activePipes = 0; // Reset the active pipes count
        finishedPipes.Clear(); // Clear the list of finished pipes

        foreach (Pipe pipe in FindObjectsOfType<Pipe>())
        {
            Destroy(pipe.gameObject); // Destroy all active pipe GameObjects
        }

        Debug.Log("ResetPipes: Restarting pipe generation."); // Debugging log
        StartCoroutine(SpawnPipe()); // Restart spawning pipes
    }
}
