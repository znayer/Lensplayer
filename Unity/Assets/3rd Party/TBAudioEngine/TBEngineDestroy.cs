using UnityEngine;
using System.Collections;

namespace TBE {

	/// <summary>
	/// Destroys the engine when the parent object of this component is destroyed
	/// </summary>
	public class TBEngineDestroy : MonoBehaviour 
	{
		void OnDestroy()
		{
			PInvAudioEngine.destroy();
		}
	}

}

