This project was created with editor version 2022.3.62f3
To enable LLM generated dialogue you need your own OpenAI API key.

Quick Setup

To import the package to unity:
1. Create a new 3D project.
2. In the top toolbar, select Assests -> Import Package -> Custom Package and pass in the project package.
3. Click Ok.

To run the project:
1. Open the Example Scene in the Scenes folder.
2. In the Hierarchy, select the LLMService GameObject.
3. In the Inspector, paste your OpenAI API key into the API Key field. !!!
4. Press Play.

How to Test
1. Start the scene.
2. Walk up to an NPC.
3. Press E to interact.
4. Click to advance to dialogue choices.
5. Select a choice.
6. The system should briefly display "Generating..."
7. The NPC response should then appear.

If the API call fails, the original line will be shown instead.
