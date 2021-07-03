using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private Neighborhood neighborhood;

    public Rigidbody rb;

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        neighborhood = GetComponent<Neighborhood>();

        pos = Random.insideUnitSphere * Spawner.SPAWN.spawnRadius; // ������� ��������� ��������� �������

        Vector3 vel = Random.onUnitSphere * Spawner.SPAWN.velocity; // ������� ��������� ��������� ��������
        rb.velocity = vel;

        LookAhead();

        Color randColor = Color.black;
        while (randColor.r + randColor.g + randColor.b < 1.0f)
        {
            randColor = new Color(Random.value, Random.value, Random.value); // �������� ����� � ��������� ����, �� �� ������� ������
        }

        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in rends)
        {
            renderer.material.color = randColor;
        }
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", randColor);
    }

    private void FixedUpdate()
    {
        Vector3 vel = rb.velocity;
        Spawner spawn = Spawner.SPAWN;

        // �������������� ������������ - �������� ������� �������
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;
        // ���� ������� ������ Vector3.zero, ������ ������������� �� ����
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spawn.velocity;
        }

        // ������������ �������� - ����������� ����������� �������� � ��������
        Vector3 velAlign = neighborhood.avgVel;
        // ������������ ���������, ������ ���� velAlign �� ����� Vector3.zero
        if (velAlign != Vector3.zero)
        {
            velAlign.Normalize(); // ��� ���������� ������ �����������, ������� ����������� ��������
            velAlign *= spawn.velocity; // � ����� ����������� � ��������� ��������
        }

        // ������������ ������� - �������� � ������� ������ ������ �������
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spawn.velocity;
        }

        // ���������� - ������������ �������� � ������� ������� Attractor
        Vector3 delta = Attractor.POS - pos;

        bool attracted = (delta.magnitude > spawn.attractPuchDist); // ���������, ���� ���������, � ������� Attractor ��� �� ����
        Vector3 velAttract = delta.normalized * spawn.velocity;

        float fixedDeltaTime = Time.fixedDeltaTime;

        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spawn.collAvoid * fixedDeltaTime);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spawn.velMatching * fixedDeltaTime);
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spawn.flockCentering * fixedDeltaTime);
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, spawn.attractPull * fixedDeltaTime);
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, spawn.attractPush * fixedDeltaTime);
                }
            }
        }

        vel = vel.normalized * spawn.velocity; // ���������� vel � ������������ c velocity

        rb.velocity = vel; // � ���������� ��������� �������� ���������� Rigidbody

        LookAhead();
    }

    private void LookAhead()
    {
        transform.LookAt(pos + rb.velocity); // ������������� ����� ������ � ����������� ������
    }
}