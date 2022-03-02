using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderScript : MonoBehaviour 
{

    public int height = 1080;
    public int width = 1920;
    private int NUMAGENTS = 100;
    public int speed = 10;
    public int brightness = 200;


    public int clock = 1;
    private Agent[] agentList;
    public ComputeShader shader;
    private RenderTexture agentTexture;

    private RenderTexture worldTexture;

    private ComputeBuffer buffer;

    private int mainKernal;
    private int updateKernal;


    public struct Agent{
        public Vector2 position;
        public Vector4 color; 
        public float speed;
        public float angle;
    }

    

    private void allocateSpace(){
        int positionSize = sizeof(float) * 2;
        int colorSize = sizeof(float) * 4;
        int speedSize = sizeof(float);
        int angleSize = sizeof(float);
        
        int totalSize = positionSize + colorSize + speedSize + angleSize; 

        buffer = new ComputeBuffer(agentList.Length, totalSize);
        buffer.SetData(agentList);
        shader.SetBuffer(mainKernal, "Agents", buffer);
        shader.SetBuffer(updateKernal, "Agents", buffer);

    }

    private void createAgents(){
        agentList = new Agent[NUMAGENTS];
        for(int i = 0; i<NUMAGENTS; i++){
            Agent agent = new Agent();
            agent.position = new Vector2(Random.Range(0, width), Random.Range(0, height));
            agent.color = new Vector4(0, 1, 1, 1);
            agent.speed = 10.0f;
            agent.angle = Random.Range(0,6.2831f);
            agentList[i] = agent;
        }
    }

    void Start()
    {
        mainKernal = shader.FindKernel("Main");
        updateKernal = shader.FindKernel("Update");

        agentTexture = new RenderTexture(width, height, 24);
        agentTexture.enableRandomWrite = true;
        agentTexture.Create();

        worldTexture = new RenderTexture(width, height, 24);
        worldTexture.enableRandomWrite = true;
        worldTexture.Create();
        
        shader.SetTexture(mainKernal, "AgentMap", agentTexture);
        shader.SetTexture(updateKernal, "AgentMap", agentTexture);
        shader.SetTexture(mainKernal, "WorldMap", worldTexture);
        shader.SetTexture(updateKernal, "WorldMap", worldTexture);
        
        createAgents();
        allocateSpace();
    }


    public void OnRenderImage(RenderTexture src, RenderTexture dest) {
        
        shader.SetInt("HEIGHT", height);
        shader.SetInt("WIDTH", width);
        shader.SetFloat("DTime", Time.deltaTime);
        shader.SetInt("clock", clock);
        shader.SetInt("NumAgents", NUMAGENTS);
        shader.SetInt("speed", speed);
        shader.SetInt("brightness", brightness);

        shader.SetTexture(mainKernal, "AgentMap", agentTexture);
        shader.SetTexture(mainKernal, "WorldMap", worldTexture);
        shader.SetBuffer(mainKernal, "Agents", buffer);

        shader.SetTexture(updateKernal, "AgentMap", agentTexture);
        shader.SetTexture(updateKernal, "WorldMap", worldTexture);
        shader.SetBuffer(updateKernal, "Agents", buffer);

        shader.Dispatch(mainKernal, agentTexture.width/16, agentTexture.height/16, 1);
        shader.Dispatch(updateKernal, agentTexture.width/16, agentTexture.height/16, 1);
        buffer.GetData(agentList);

        Graphics.CopyTexture(worldTexture, agentTexture);
        Graphics.Blit(agentTexture, dest);
        clock++;
        
    }
}
