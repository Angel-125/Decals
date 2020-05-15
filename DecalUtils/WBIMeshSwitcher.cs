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
    public class WBIMeshSwitcher: PartModule
    {
        #region Fields
        /// <summary>
        /// List of part variants.
        /// </summary>
        [SerializeField]
        public List<PartVariant> partVariants;

        /// <summary>
        /// Currently selected index from the part variant list.
        /// </summary>
        [UI_VariantSelector(affectSymCounterparts = UI_Scene.None, controlEnabled = true, scene = UI_Scene.Editor)]
        [KSPField(isPersistant = true)]
        private int selectedIndex;
        #endregion

        #region Overrides
        public override void OnAwake()
        {
            base.OnAwake();

            //This is only supported in KSP 1.9. To support surface attachment variants, be sure to define a NODES config node with node_attach.
            //this.part.baseVariant = new PartVariant("Base", "Base", this.part.attachNodes, this.part.srfAttachNode);

            //We'll use what's in KSP 1.8.1 instead...
            this.part.baseVariant = new PartVariant("Base", "Base", this.part.attachNodes);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            loadVariants(node);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            loadVariants();
            if (selectedIndex < 0)
                selectedIndex = 0;
            updateMeshes();

            if (!HighLogic.LoadedSceneIsEditor)
                return;

            UI_VariantSelector variantSelector = this.Fields["selectedIndex"].uiControlEditor as UI_VariantSelector;
            variantSelector.variants = this.partVariants;
            variantSelector.onFieldChanged = variantSelector.onFieldChanged + new Callback<BaseField, object>(this.onVariantChanged);
        }
        #endregion

        #region Helpers
        private void onVariantChanged(BaseField field, object obj)
        {
            updateMeshes();
        }

        /// <summary>
        /// Loads all the part variants from the part's config node.
        /// </summary>
        private void loadVariants()
        {
            if (this.part.partInfo.partConfig == null)
                return;
            ConfigNode[] nodes = this.part.partInfo.partConfig.GetNodes("MODULE");
            ConfigNode switcherNode = null;
            ConfigNode node = null;
            string moduleName;
            List<string> optionNamesList = new List<string>();

            //Get the switcher config node.
            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];
                if (node.HasValue("name"))
                {
                    moduleName = node.GetValue("name");
                    if (moduleName == this.ClassName)
                    {
                        switcherNode = node;
                        break;
                    }
                }
            }
            if (switcherNode == null)
                return;

            loadVariants(switcherNode);
        }

        /// <summary>
        /// Loads all the part variants from the part config node.
        /// </summary>
        /// <param name="node">The node to search for VARIANT nodes.</param>
        private void loadVariants(ConfigNode node)
        {
            if (partVariants == null && node.HasNode("VARIANT"))
            {
                partVariants = new List<PartVariant>();
                ConfigNode[] variantNodes = node.GetNodes("VARIANT");
                ConfigNode variantNode;
                PartVariant partVariant;

                for (int index = 0; index < variantNodes.Length; index++)
                {
                    variantNode = variantNodes[index];
                    partVariant = new PartVariant(this.part.baseVariant);
                    partVariant.Load(variantNode);
                    partVariants.Add(partVariant);
                }
            }
        }

        /// <summary>
        /// Updates the meshes, ignoring symmetry.
        /// </summary>
        private void updateMeshes()
        {
            if (selectedIndex >= 0 && partVariants.Count > 0)
            {
                PartVariant selectedVariant = partVariants[selectedIndex];
                Material[] materials = new Material[] { };

                ModulePartVariants.ApplyVariant(this.part, this.part.transform, selectedVariant, materials, false);
            }
        }
        #endregion
    }
}
