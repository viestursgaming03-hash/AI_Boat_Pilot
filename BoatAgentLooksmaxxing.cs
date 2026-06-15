using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using random = UnityEngine.Random;
using Unity.Mathematics;

// for training - conda activate mlagents
// for training - cd "C:\Users\User\AI_boat_pilot"
// for training - mlagents-learn --help
// for training - mlagents-learn config/Turtle.yaml --run-id=
// for training - tensorboard --logdir results

public class BoatAgentLooksmaxxing : Agent
{
    [SerializeField] private Transform _goal;
    [SerializeField] private SpriteRenderer _arrow;
    [SerializeField] private ParticleSystem _oarRight;
    [SerializeField] private ParticleSystem _waterRight;
    [SerializeField] private ParticleSystem _oarLeft;
    [SerializeField] private ParticleSystem _waterLeft;
    [SerializeField] private ParticleSystem _oarRightBreak;
    [SerializeField] private ParticleSystem _oarLeftBreak;
    [SerializeField] private float _moveSpeed = 0.525f;
    [SerializeField] private float _rotationSpeed = 135f;

    Rigidbody2D _rb;
    private ParticleSystem _oarRightInstance;
    private ParticleSystem _waterRightInstance;
    private ParticleSystem _oarLeftInstance;
    private ParticleSystem _waterLeftInstance;
    private ParticleSystem _oarRightBreakInstance;
    private ParticleSystem _oarLeftBreakInstance;
    private Vector3 _velocity = new(0f, 0f, 0f);
    private int _ones = 0;
    private int _twos = 0;
    private int _threes = 0;
    private int _fours = 0;
    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;
    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentEpisode = 0;
        _cumulativeReward = 0f;
        _velocity = new Vector3(0f, 0f, 0f);
    }

    public override void OnEpisodeBegin()
    {
        _currentEpisode++;
        _cumulativeReward = 0f;
        _velocity = new Vector3(0f, 0f, 0f);
        _rb.angularVelocity = 0f;
        _rb.linearVelocity = new Vector2(0f, 0f);
        _ones = 0;
        _twos = 0;
        _threes = 0;
        _fours = 0;

        SpawnObjects();
    }

    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector2(0f, 0f);

        // decides goal posiotion in a doughnut shape zone
        float randomDirection = random.Range(0f, 360f);
        float randomDistance = random.Range(10f, 35f);
        Vector3 goalPosition = transform.localPosition + new Vector3(math.cos(math.radians(randomDirection)) * randomDistance, math.sin(math.radians(randomDirection)) * randomDistance, 0f);

        // applies goal position
        _goal.localPosition = new Vector3(goalPosition.x, goalPosition.y, 0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // boat's position normalized
        float boatPosX_normailized = transform.localPosition.x / 50f;
        float boatPosY_normailized = transform.localPosition.y / 50f;

        // boat's rotation normalized
        float boatRotation_normalized = transform.localRotation.eulerAngles.z / 360f * 2f - 1f;

        // observations
        sensor.AddObservation(boatPosX_normailized);
        sensor.AddObservation(boatPosY_normailized);
        sensor.AddObservation(boatRotation_normalized);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0; // nothing by default
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        if (Input.GetKey(KeyCode.F))
        {
            discreteActionsOut[0] = 4;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // move agent with action
        MoveAgent(actions.DiscreteActions);

        // encouraging penalty
        AddReward(-5f / MaxStep); // with 10000 steps its 0.0005

        // update cumulative reward
        _cumulativeReward = GetCumulativeReward();
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];
        transform.localPosition += _velocity;
        _arrow.color = new Color(_arrow.color.r, _arrow.color.g, _arrow.color.b, _velocity.magnitude * 4f);
        ManageVelocity();
        //AddReward(math.max(_velocity.magnitude - 0.2f, 0) * 100f / MaxStep); // velocity peak is about 0.25 so up to 5 / maxstep
        //AddReward(math.max(math.pow((30f - new Vector2(transform.localPosition.x - _goal.localPosition.x, transform.localPosition.y - _goal.localPosition.y).magnitude) / 10f, 2), 0f) / MaxStep); // gives exponential reward for proximity, up to about 7.5 / maxstep

        switch (action)
        {
            case 1: // row with left Oar
                _velocity += new Vector3(_moveSpeed * math.cos(math.radians(transform.localRotation.eulerAngles.z)) * Time.deltaTime, _moveSpeed * math.sin(math.radians(transform.localRotation.eulerAngles.z)) * Time.deltaTime, 0f);
                _rb.angularVelocity -= _rotationSpeed * Time.deltaTime;
                SpawnRowingParticles(true);
                _ones++;
                break;
            case 2: // row with right Oar
                _velocity += new Vector3(_moveSpeed * math.cos(math.radians(transform.localRotation.eulerAngles.z)) * Time.deltaTime, _moveSpeed * math.sin(math.radians(transform.localRotation.eulerAngles.z)) * Time.deltaTime, 0f);
                _rb.angularVelocity += _rotationSpeed * Time.deltaTime;
                SpawnRowingParticles(false);
                _twos++;
                break;
            case 3: // break with left Oar
                ManageVelocity();
                _rb.angularVelocity += _velocity.magnitude * _rotationSpeed * Time.deltaTime * 10f;
                SpawnBreakingParticles(true);
                _threes++;
                break;
            case 4: // break with right Oar
                ManageVelocity();
                _rb.angularVelocity -= _velocity.magnitude * _rotationSpeed * Time.deltaTime * 10f;
                SpawnBreakingParticles(false);
                _fours++;
                break;
        }
    }

    private void ManageVelocity()
    {
        _velocity *= 0.96f;
        if (_velocity.magnitude < 0.01f)
        {
            _velocity = new Vector3(0f, 0f, 0f);
        }
    }

    private void SpawnRowingParticles(bool isLeft)
    {
        Quaternion spawnDirection = transform.localRotation;

        if (isLeft)
            if (_oarLeftInstance == null && _oarLeftBreakInstance == null)
            {
                _oarLeftInstance = Instantiate(_oarLeft, transform.localPosition, spawnDirection, transform);
                _waterLeftInstance = Instantiate(_waterLeft, transform.localPosition, spawnDirection, _oarLeftInstance.transform);
            }

        if (!isLeft)
            if (_oarRightInstance == null && _oarRightBreakInstance == null)
            {
                _oarRightInstance = Instantiate(_oarRight, transform.localPosition, spawnDirection, transform);
                _waterRightInstance = Instantiate(_waterRight, transform.localPosition, spawnDirection, _oarRightInstance.transform);
            }
    }

    private void SpawnBreakingParticles(bool isLeft)
    {
        Quaternion spawnDirection = transform.localRotation;

        if (isLeft)
             _oarLeftBreakInstance = Instantiate(_oarLeftBreak, transform.localPosition, spawnDirection, transform);
        if (!isLeft)
            _oarRightBreakInstance = Instantiate(_oarRightBreak, transform.localPosition, spawnDirection, transform);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            GoalReached();
        }
    }

    private void GoalReached()
    {
        AddReward(6f); // large reward
        _cumulativeReward = GetCumulativeReward();

        print("ones: " + _ones + " twos: " + _twos + " threes: " + _threes + " fours: " + _fours);

        EndEpisode();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boundrie"))
        {
            // punish for hitting wall
            AddReward(-0.02f);

            // push away from wall
            _velocity = new Vector3(0f, 0f, 0f);
            _velocity += new Vector3(transform.localPosition.x / -500, transform.localPosition.y / -500, 0f);
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boundrie"))
        {
            // keep punishing for continuous collision
            AddReward(-0.01f * Time.deltaTime);
        }
    }
}
