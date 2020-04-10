using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code copyright 2020, by Michael Billard (Angel-125)
License: GPLV3

Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.
Note that Wild Blue Industries is a ficticious entity 
created for entertainment purposes. It is in no way meant to represent a real entity.
Any similarity to a real entity is purely coincidental.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace DecalUtils
{
    /// <summary>
    /// This was originally a name-tag module for changing the names of the Heisenberg airships. Now it's adapted for decals in general.
    /// </summary>
    public class WBIDecal : PartModule
    {
        /// <summary>
        /// URL to the image that's displayed by the decal.
        /// </summary>
        [KSPField(isPersistant = true)]
        public string decalURL = string.Empty;

        /// <summary>
        /// Flag to indicate whether or not the decal is visible
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool isVisible;

        /// <summary>
        /// Override flag to ensure that the decal is always visible.
        /// </summary>
        [KSPField()]
        public bool alwaysVisible;

        /// <summary>
        /// Flag to indicate if the decal updates symmetry parts
        /// </summary>
        [KSPField()]
        public bool updateSymmetry = true;

        /// <summary>
        /// GUI name for button that toggles decal visibility
        /// </summary>
        [KSPField()]
        public string toggleDecalName = "Toggle Decal";

        /// <summary>
        ///  GUI name for button that selects the decal.
        /// </summary>
        [KSPField()]
        public string selectDecalName = "Select Decal";

        /// <summary>
        /// List of transforms that will be changed by the decal. Separate names by semicolon
        /// </summary>
        [KSPField()]
        public string decalTransforms = string.Empty;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Events["ToggleDecal"].guiName = toggleDecalName;
            Events["ToggleDecal"].active = !alwaysVisible;

            Events["SelectDecal"].guiName = selectDecalName;

            ChangeDecal();
        }

        /// <summary>
        /// Toggles visibility of the decal.
        /// </summary>
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Toggle Decal")]
        public void ToggleDecal()
        {
            isVisible = !isVisible;
            ChangeDecal();
            updateSymmetryParts();
        }

        /// <summary>
        /// Changes the decal
        /// </summary>
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Select Decal")]
        public void SelectDecal()
        {
            decalURL = HighLogic.CurrentGame.flagURL;
            FlagBrowser flagBrowser = (UnityEngine.Object.Instantiate((UnityEngine.Object)(new FlagBrowserGUIButton(null, null, null, null)).FlagBrowserPrefab) as GameObject).GetComponent<FlagBrowser>();
            flagBrowser.OnFlagSelected = onFlagSelected;
        }

        /// <summary>
        /// Private event handler to respond to flag selection.
        /// </summary>
        /// <param name="selected">The selected texture</param>
        private void onFlagSelected(FlagBrowser.FlagEntry selected)
        {
            decalURL = selected.textureInfo.name;
            ChangeDecal();
            updateSymmetryParts();
        }

        /// <summary>
        /// Updates symmetry parts with the new decal
        /// </summary>
        protected void updateSymmetryParts()
        {
            if (updateSymmetry)
            {
                WBIDecal nameTag;
                foreach (Part symmetryPart in this.part.symmetryCounterparts)
                {
                    nameTag = symmetryPart.GetComponent<WBIDecal>();
                    nameTag.decalURL = this.decalURL;
                    nameTag.ChangeDecal();
                }
            }
        }

        /// <summary>
        /// Changes the decal on all named transforms.
        /// </summary>
        public void ChangeDecal()
        {
            string[] tagTransforms = decalTransforms.Split(';');
            Transform[] targets;
            Texture textureForDecal;
            Renderer rendererMaterial;

            foreach (string transform in tagTransforms)
            {
                //Get the targets
                targets = part.FindModelTransforms(transform);
                if (targets == null)
                {
                    Debug.Log("No targets found for " + transform);
                    return;
                }

                foreach (Transform target in targets)
                {
                    target.gameObject.SetActive(isVisible);
                    Collider collider = target.gameObject.GetComponent<Collider>();
                    if (collider != null)
                        collider.enabled = isVisible;

                    if (string.IsNullOrEmpty(decalURL) == false)
                    {
                        rendererMaterial = target.GetComponent<Renderer>();

                        textureForDecal = GameDatabase.Instance.GetTexture(decalURL, false);
                        if (textureForDecal != null)
                            rendererMaterial.material.SetTexture("_MainTex", textureForDecal);
                    }
                }
            }
        }

    }
}