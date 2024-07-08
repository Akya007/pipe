using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pipe : MonoBehaviour
{
    public int maximumPipeTurns = 200; // Maximum number of turns a pipe can make before stopping
    public float minimumStretchDistance = 3f; // Minimum distance a pipe segment can stretch
    public float maximumStretchDistance = 10f; // Maximum distance a pipe segment can stretch
    public float speed = 0.01f; // Speed at which the pipe grows
    public float pipeRadius = 0.5f; // Radius of the pipe segments
    public float turnSphereRadius = 0.7f; // Radius of the spheres at the turns
    public int turnFrequency = 5; // Number of segments before a turn
    public Color pipeColor; // Color of the pipe
    public bool isGenerating = false; // Flag to check if the pipe is currently generating

    private List<GameObject> pipeSegments = new List<GameObject>(); // List to hold all the pipe segments and spheres
    private bool isPaused = false; // Flag to check if the pipe is paused
    private Vector3 lastDirection = Vector3.forward; // Store the last direction to avoid moving backwards

    public delegate void PipeFinished(Pipe pipe); // Delegate for when a pipe is finished
    public event PipeFinished OnPipeFinished; // Event triggered when the pipe is finished

    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>(); // Set to keep track of occupied positions to avoid collisions

    public Vector3 boundarySize; // Boundary size for the pipes

    void Start()
    {
        pipeColor = GetRandomColor(); // Set a random color for the pipe
        StartCoroutine(GeneratePipes()); // Start generating pipes
    }

    IEnumerator GeneratePipes()
    {
        isGenerating = true; // Set the generating flag to true
        Vector3 currentPosition = Vector3.zero; // Starting position of the pipe
        Vector3 direction = GetRandomForwardDirection(lastDirection); // Get a random initial direction
        int segments = 0; // Counter for the number of segments
        int turns = 0; // Counter for the number of turns

        while (turns < maximumPipeTurns)
        {
            float distance = Random.Range(minimumStretchDistance, maximumStretchDistance); // Random distance for the next segment
            Vector3 targetPosition = currentPosition + direction * distance; // Calculate the target position

            if (CheckCollision(currentPosition, targetPosition) || !IsWithinBoundary(targetPosition)) // Check if the new segment collides with anything or is out of boundary
            {
                direction = GetRandomForwardDirection(lastDirection); // Get a new direction
                CreateTurn(ref currentPosition, ref direction); // Create a turn at the current position
                turns++; // Increment the turn counter
                segments = 0; // Reset the segment counter after a turn
                continue; // Skip to the next iteration
            }

            CreateSegment(ref currentPosition, targetPosition); // Create the new segment
            currentPosition = targetPosition; // Update the current position to the target position
            lastDirection = direction; // Update the last direction
            segments++; // Increment the segment counter

            if (segments >= turnFrequency) // Check if it's time to make a turn
            {
                direction = GetRandomForwardDirection(lastDirection); // Get a new direction
                CreateTurn(ref currentPosition, ref direction); // Create a turn at the current position
                segments = 0; // Reset the segment counter after a turn
                turns++; // Increment the turn counter
            }

            yield return new WaitForSeconds(1f / speed); // Wait before generating the next segment
        }

        isGenerating = false; // Set the generating flag to false
        EndPipe(); // End the pipe when maximum turns are reached
    }

    Vector3 GetRandomForwardDirection(Vector3 lastDirection)
    {
        // List of possible directions for the pipe to move
        List<Vector3> directions = new List<Vector3> {
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right,
            Vector3.up, Vector3.down
        };

        // Remove the opposite direction of the last movement
        directions.Remove(-lastDirection);

        return directions[Random.Range(0, directions.Count)]; // Return a random direction from the list
    }

    bool CheckCollision(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized; // Calculate the direction of the segment
        float distance = Vector3.Distance(start, end); // Calculate the distance of the segment
        Ray ray = new Ray(start, direction); // Create a ray from the start position in the direction of the segment
        return Physics.Raycast(ray, distance); // Check if the ray hits anything within the distance
    }

    void CreateSegment(ref Vector3 start, Vector3 end)
    {
        // Create a cylinder to represent the pipe segment
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = (start + end) / 2; // Position the cylinder at the midpoint between start and end
        cylinder.transform.localScale = new Vector3(pipeRadius, Vector3.Distance(start, end) / 2, pipeRadius); // Scale the cylinder to the correct length and thickness
        cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start); // Rotate the cylinder to align with the segment direction
        cylinder.GetComponent<Renderer>().material.color = pipeColor; // Set the color of the cylinder
        cylinder.transform.parent = transform; // Set the pipe object as the parent of the cylinder
        pipeSegments.Add(cylinder); // Add the cylinder to the list of pipe segments

        occupiedPositions.Add(start); // Mark the start position as occupied
        occupiedPositions.Add(end); // Mark the end position as occupied
    }

    void CreateTurn(ref Vector3 position, ref Vector3 direction)
    {
        // Create a sphere to represent the turn
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position; // Position the sphere at the current position
        sphere.transform.localScale = Vector3.one * turnSphereRadius; // Scale the sphere to the correct size
        sphere.GetComponent<Renderer>().material.color = pipeColor; // Set the color of the sphere
        sphere.transform.parent = transform; // Set the pipe object as the parent of the sphere
        pipeSegments.Add(sphere); // Add the sphere to the list of pipe segments

        occupiedPositions.Add(position); // Mark the turn position as occupied

        direction = GetRandomForwardDirection(lastDirection); // Get a new direction for the next segment
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed; // Set the speed of the pipe
    }

    public void SetMaxTurns(int newMaxTurns)
    {
        maximumPipeTurns = newMaxTurns; // Set the maximum number of turns for the pipe
    }

    public bool IsPaused()
    {
        return isPaused; // Return the paused state of the pipe
    }

    public void EndPipe()
    {
        OnPipeFinished?.Invoke(this); // Trigger the OnPipeFinished event
    }

    private Color GetRandomColor()
    {
        Color color;
        do
        {
            color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f); // Generate lighter colors by setting hue, saturation, and value ranges
        } while (color.r < 0.2f && color.g < 0.2f && color.b < 0.2f); // Avoid very dark colors

        return color; // Return the generated color
    }

    bool IsWithinBoundary(Vector3 position)
    {
        // Check if the position is within the boundary
        return Mathf.Abs(position.x) <= boundarySize.x / 2 &&
               Mathf.Abs(position.y) <= boundarySize.y / 2 &&
               Mathf.Abs(position.z) <= boundarySize.z / 2;
    }
}
