﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SSTUTools.Module
{
    class SSTUInterstageDecoupler : ModuleDecouple
    {
        [KSPField]
        public String modelName = "SSTU/Assets/SC-ENG-ULLAGE-A";

        [KSPField]
        public float defaultModelScale = 5f;

        [KSPField]
        public float resourceVolume = 0.25f;

        [KSPField]
        public float engineMass = 0.15f;

        [KSPField]
        public float engineThrust = 300f;

        [KSPField]
        public bool scaleThrust = true;

        [KSPField]
        public float thrustScalePower = 2f;

        [KSPField]
        public bool useRF = false;

        [KSPField]
        public String baseTransformName = "InterstageDecouplerRoot";

        [KSPField]
        public String diffuseTextureName = "SC-GEN-Fairing-DIFF";

        [KSPField]
        public int cylinderSides = 24;

        [KSPField]
        public int numberOfPanels = 1;

        [KSPField]
        public float wallThickness = 0.05f;

        [KSPField]
        public int numberOfEngines = 4;

        [KSPField]
        public float engineRotationOffset = 90f;

        [KSPField]
        public float engineHeight = 0.8f;

        [KSPField]
        public float engineVerticalOffset = 0.4f;

        [KSPField]
        public float enginePlacementAngleOffset = 45f;

        [KSPField]
        public int engineModuleIndex = 1;

        [KSPField]
        public int upperDecouplerModuleIndex = 2;

        [KSPField]
        public float minDiameter = 0.625f;

        [KSPField]
        public float maxDiameter = 20f;

        [KSPField]
        public float maxHeight = 10f;

        [KSPField]
        public float diameterIncrement = 0.625f;

        [KSPField]
        public float heightIncrement = 1.0f;

        [KSPField]
        public float taperHeightIncrement = 1.0f;

        [KSPField]
        public float autoDecoupleDelay = 4f;

        [KSPField(isPersistant = true)]
        public float currentHeight = 1.0f;

        [KSPField(isPersistant = true)]
        public float currentTopDiameter = 2.5f;

        [KSPField(isPersistant = true)]
        public float currentBottomDiameter = 2.5f;

        [KSPField(isPersistant = true)]
        public float currentTaperHeight = 0.0f;

        [KSPField(isPersistant = true)]
        public bool initializedResources = false;

        [KSPField(guiActiveEditor = true, guiActive =true, guiName = "Top Diameter")]
        public float guiTopDiameter;

        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Bottom Diameter")]
        public float guiBottomDiameter;

        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Taper Height")]
        public float guiTaperHeight;

        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Total Thrust")]
        public float guiEngineThrust;

        [KSPField(guiActiveEditor = true, guiName = "Top Diam. Adj"), UI_FloatRange(minValue = 0f, stepIncrement = 0.05f, maxValue = 0.95f)]
        public float editorTopDiameterAdjust;

        [KSPField(guiActiveEditor = true, guiName = "Bottom Diam. Adj"), UI_FloatRange(minValue = 0f, stepIncrement = 0.05f, maxValue = 0.95f)]
        public float editorBottomDiameterAdjust;

        [KSPField(guiActiveEditor = true, guiName = "Height Adj"), UI_FloatRange(minValue = 0f, stepIncrement = 0.05f, maxValue = 0.95f)]
        public float editorHeightAdjust;

        [KSPField(guiActiveEditor = true, guiName = "Taper Height Adj"), UI_FloatRange(minValue = 0f, stepIncrement = 0.05f, maxValue = 0.95f)]
        public float editorTaperHeightAdjust;

        [KSPField(isPersistant = true)]
        public bool invertEngines = false;

        [KSPField(isPersistant = true, guiName = "Auto Decouple", guiActive =true, guiActiveEditor =true)]
        public bool autoDecouple = false;

        [Persistent]
        public String configNodeData = String.Empty;

        private float remainingDelay;

        private float editorTopDiameter;
        private float editorBottomDiameter;
        private float editorTaperHeight;
        private float editorHeight;

        private float prevHeightAdjust;
        private float prevTopDiameterAdjust;
        private float prevBottomDiameterAdjust;
        private float prevTaperHeightAdjust;

        private Material fairingMaterial;
        private InterstageDecouplerModel fairingBase;
        private InterstageDecouplerEngine[] engineModels;
        private UVArea outsideUV;
        private UVArea insideUV;
        private UVArea edgesUV;

        private FuelTypeData fuelType;

        private TechLimitHeightDiameter[] techLimits;
        private float techLimitMaxHeight;
        private float techLimitMaxDiameter;

        private bool initialized = false;

        private ModuleEngines engineModule;
        private ModuleDecouple decoupler;

        [KSPEvent(guiName = "Top Diameter -", guiActiveEditor = true)]
        public void prevTopDiameterEvent()
        {
            setTopDiameterFromEditor(currentTopDiameter - diameterIncrement, true);
        }

        [KSPEvent(guiName = "Top Diameter +", guiActiveEditor = true)]
        public void nextTopDiameterEvent()
        {
            setTopDiameterFromEditor(currentTopDiameter + diameterIncrement, true);
        }

        [KSPEvent(guiName = "Bottom Diameter -", guiActiveEditor = true)]
        public void prevBottomDiameterEvent()
        {
            setBottomDiameterFromEditor(currentBottomDiameter - diameterIncrement, true);
        }

        [KSPEvent(guiName = "Bottom Diameter +", guiActiveEditor = true)]
        public void nextBottomDiameterEvent()
        {
            setBottomDiameterFromEditor(currentBottomDiameter + diameterIncrement, true);
        }

        [KSPEvent(guiName = "Height -", guiActiveEditor = true)]
        public void prevHeightEvent()
        {
            setHeightFromEditor(currentHeight - heightIncrement, true);
        }

        [KSPEvent(guiName = "Height +", guiActiveEditor = true)]
        public void nextHeightEvent()
        {
            setHeightFromEditor(currentHeight + heightIncrement, true);
        }

        [KSPEvent(guiName = "Taper Height -", guiActiveEditor =true)]
        public void prevTaperHeightEvent()
        {
            setTaperHeightFromEditor(currentTaperHeight - heightIncrement, true);
        }

        [KSPEvent(guiName = "Taper Height +", guiActiveEditor = true)]
        public void nextTaperHeightEvent()
        {
            setTaperHeightFromEditor(currentTaperHeight + heightIncrement, true);
        }

        [KSPEvent(guiName = "Invert Engines", guiActiveEditor = true)]
        public void invertEnginesEvent()
        {
            invertEnginesFromEditor(true);
        }

        [KSPEvent(guiName = "Toggle Auto Decouple", guiActiveEditor = true)]
        public void toggleAutoDecoupleEvent()
        {
            autoDecouple = !autoDecouple;
            foreach (Part p in part.symmetryCounterparts)
            {
                p.GetComponent<SSTUInterstageDecoupler>().autoDecouple = this.autoDecouple;
            }
        }

        private void setTaperHeightFromEditor(float newHeight, bool updateSymmetry)
        {
            if (newHeight > currentHeight) { newHeight = currentHeight; }
            if (newHeight > maxHeight) { newHeight = maxHeight; }
            if (newHeight > techLimitMaxHeight) { newHeight = techLimitMaxHeight; }
            float minTaperHeight = engineHeight * getEngineScale();
            if (newHeight < minTaperHeight) { newHeight = minTaperHeight; }
            currentTaperHeight = newHeight;
            updateEditorFields();
            buildFairing();
            updateGuiFields();
            updateNodePositions(true);
            updatePartMass();
            if (updateSymmetry)
            {
                SSTUInterstageDecoupler idc;
                foreach (Part p in part.symmetryCounterparts)
                {
                    idc = part.GetComponent<SSTUInterstageDecoupler>();
                    idc.setTaperHeightFromEditor(newHeight, false);
                }
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        private void setHeightFromEditor(float newHeight, bool updateSymmetry)
        {
            if (newHeight > maxHeight) { newHeight = maxHeight; }
            if (newHeight > techLimitMaxHeight) { newHeight = techLimitMaxHeight; }
            float minTaperHeight = engineHeight * getEngineScale();
            if (newHeight < minTaperHeight) { newHeight = minTaperHeight; }
            currentHeight = newHeight;
            updateEditorFields();
            buildFairing();
            updateGuiFields();
            updateNodePositions(true);
            updatePartMass();
            if (updateSymmetry)
            {
                SSTUInterstageDecoupler idc;
                foreach (Part p in part.symmetryCounterparts)
                {
                    idc = part.GetComponent<SSTUInterstageDecoupler>();
                    idc.setHeightFromEditor(newHeight, false);
                }
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        private void setTopDiameterFromEditor(float newDiameter, bool updateSymmetry)
        {
            if (newDiameter > maxDiameter) { newDiameter = maxDiameter; }
            if (newDiameter > techLimitMaxDiameter) { newDiameter = techLimitMaxDiameter; }
            if (newDiameter < minDiameter) { newDiameter = minDiameter; }
            currentTopDiameter = newDiameter;
            updateEditorFields();
            buildFairing();
            updateGuiFields();
            updateNodePositions(true);
            updatePartMass();
            if (updateSymmetry)
            {
                SSTUInterstageDecoupler idc;
                foreach (Part p in part.symmetryCounterparts)
                {
                    idc = part.GetComponent<SSTUInterstageDecoupler>();
                    idc.setTopDiameterFromEditor(newDiameter, false);
                }
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        private void setBottomDiameterFromEditor(float newDiameter, bool updateSymmetry)
        {
            if (newDiameter > maxDiameter) { newDiameter = maxDiameter; }
            if (newDiameter > techLimitMaxDiameter) { newDiameter = techLimitMaxDiameter; }
            if (newDiameter < minDiameter) { newDiameter = minDiameter; }
            currentBottomDiameter = newDiameter;
            float minHeight = getEngineScale() * engineHeight;
            if (currentHeight < minHeight) { currentHeight = minHeight; }
            if (currentTaperHeight < minHeight){currentTaperHeight = minHeight;}

            updateEditorFields();
            buildFairing();
            updateGuiFields();
            updateNodePositions(true);
            updateResources();
            updatePartMass();
            updateEngineThrust();
            if (updateSymmetry)
            {
                SSTUInterstageDecoupler idc;
                foreach (Part p in part.symmetryCounterparts)
                {
                    idc = part.GetComponent<SSTUInterstageDecoupler>();
                    idc.setBottomDiameterFromEditor(newDiameter, false);
                }
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        private void invertEnginesFromEditor(bool updateSymmetry)
        {
            invertEngines = !invertEngines;
            updateEnginePositionAndScale();
            if (updateSymmetry)
            {
                SSTUInterstageDecoupler idc;
                foreach (Part p in part.symmetryCounterparts)
                {
                    idc = part.GetComponent<SSTUInterstageDecoupler>();
                    idc.invertEngines = invertEngines;
                    idc.updateEnginePositionAndScale();
                }
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (node.HasNode("TECHLIMIT")) { configNodeData = node.ToString(); }
            initialize();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            initialize();
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorShipModified.Add(new EventData<ShipConstruct>.OnEvent(onEditorVesselModified));
            }
        }

        public void Start()
        {
            engineModule = part.GetComponent<ModuleEngines>();
            ModuleDecouple[] decouplers = part.GetComponents<ModuleDecouple>();
            foreach (ModuleDecouple dc in decouplers)
            {
                if (dc != this) { decoupler = dc; break; }
            }
            updateEngineThrust();
        }

        public void FixedUpdate()
        {
            if (remainingDelay > 0)
            {
                remainingDelay -= TimeWarp.fixedDeltaTime;
                if (remainingDelay <= 0)
                {
                    if (!isDecoupled) { Decouple(); }
                    if (!decoupler.isDecoupled) { decoupler.Decouple(); }
                }
            }
            else if (autoDecouple && engineModule.flameout)
            {
                if (autoDecoupleDelay > 0)
                {
                    remainingDelay = autoDecoupleDelay;
                }
                else
                {
                    if (!isDecoupled) { Decouple(); }
                    if (!decoupler.isDecoupled) { decoupler.Decouple(); }
                }
            }
        }
        
        public void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorShipModified.Remove(new EventData<ShipConstruct>.OnEvent(onEditorVesselModified));
            }
        }

        public void onEditorVesselModified(ShipConstruct ship)
        {
            if (prevBottomDiameterAdjust != editorBottomDiameterAdjust)
            {
                setBottomDiameterFromEditor( editorBottomDiameter + editorBottomDiameterAdjust * diameterIncrement, true );
            }
            if (prevTopDiameterAdjust != editorTopDiameterAdjust)
            {
                setTopDiameterFromEditor(editorTopDiameter + editorTopDiameterAdjust * diameterIncrement, false);
            }
            if (prevHeightAdjust != editorHeightAdjust)
            {
                setHeightFromEditor(editorHeight + editorHeightAdjust * heightIncrement, false);
            }
            if (prevTaperHeightAdjust != editorTaperHeightAdjust)
            {
                setTaperHeightFromEditor(editorTaperHeight + editorTaperHeightAdjust * heightIncrement, false);
            }
        }

        private void initialize()
        {
            if (initialized) { return; }
            initialized = true;
            fairingMaterial = SSTUUtils.loadMaterial(diffuseTextureName, String.Empty, "KSP/Specular");
            ConfigNode node = SSTUConfigNodeUtils.parseConfigNode(configNodeData);
            techLimits = TechLimitHeightDiameter.loadTechLimits(node.GetNodes("TECHLIMIT"));
            outsideUV = new UVArea(node.GetNode("UVMAP", "name", "outside"));
            insideUV = new UVArea(node.GetNode("UVMAP", "name", "inside"));
            edgesUV = new UVArea(node.GetNode("UVMAP", "name", "edges"));
            updateTechLimits();
            
            fuelType = new FuelTypeData(node.GetNode("FUELTYPE"));

            Transform modelBase = part.transform.FindRecursive("model");

            setupEngineModels(modelBase);

            Transform root = modelBase.FindOrCreate(baseTransformName);
            Transform collider = modelBase.FindOrCreate("InterstageFairingBaseCollider");
            fairingBase = new InterstageDecouplerModel(root.gameObject, collider.gameObject, 0.25f, cylinderSides, numberOfPanels, wallThickness);
            buildFairing();
            updateNodePositions(false);
            updateEditorFields();

            if (!initializedResources && (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor))
            {
                initializedResources = true;
                updateResources();
            }
            updatePartMass();
            updateEngineThrust();
            updateGuiFields();
        }

        private void updateEditorFields()
        {
            float div = currentTopDiameter / diameterIncrement;
            float whole = (int)div;
            float extra = div - whole;
            editorTopDiameter = whole * diameterIncrement;
            editorTopDiameterAdjust = prevTopDiameterAdjust = extra;

            div = currentBottomDiameter / diameterIncrement;
            whole = (int)div;
            extra = div - whole;
            editorBottomDiameter = whole * diameterIncrement;
            editorBottomDiameterAdjust = prevBottomDiameterAdjust = extra;

            div = currentHeight / heightIncrement;
            whole = (int)div;
            extra = div - whole;
            editorHeight = whole * heightIncrement;
            editorHeightAdjust = prevHeightAdjust = extra;
            
            div = currentTaperHeight / heightIncrement;
            whole = (int)div;
            extra = div - whole;
            editorTaperHeight = whole * heightIncrement;
            editorTaperHeightAdjust = prevTaperHeightAdjust = extra;
        }

        private void setupEngineModels(Transform modelBase)
        {
            engineModels = new InterstageDecouplerEngine[numberOfEngines];
            float anglePerEngine = 360f / (float)numberOfEngines;
            float startAngle = enginePlacementAngleOffset;
            Transform modelTransform;
            String fullName;
            float placementAngle;
            for (int i = 0; i < numberOfEngines; i++)
            {
                placementAngle = startAngle + ((float)i * anglePerEngine);
                fullName = modelName + "-" + i;
                modelTransform = modelBase.FindRecursive(fullName);
                if (modelTransform == null)
                {
                    modelTransform = SSTUUtils.cloneModel(modelName).transform;
                    modelTransform.name = fullName;
                    modelTransform.gameObject.name = fullName;
                }
                modelTransform.parent = modelBase;
                engineModels[i] = new InterstageDecouplerEngine(modelTransform, placementAngle, engineRotationOffset);
            }
        }

        private void buildFairing()
        {
            fairingBase.clearProfile();

            fairingBase.outsideUV = outsideUV;
            fairingBase.insideUV = insideUV;
            fairingBase.edgesUV = edgesUV;

            float halfHeight = currentHeight * 0.5f;
            fairingBase.addRing(-halfHeight, currentBottomDiameter * 0.5f);
            if (currentTaperHeight > 0)
            {
                fairingBase.addRing(-halfHeight + currentTaperHeight, currentBottomDiameter * 0.5f);
            }
            if (currentTaperHeight < currentHeight)
            {
                fairingBase.addRing(halfHeight, currentTopDiameter * 0.5f);
            }

            fairingBase.generateFairing();
            fairingBase.setMaterial(fairingMaterial);
            fairingBase.setOpacity(HighLogic.LoadedSceneIsEditor ? 0.25f : 1.0f);

            updateEnginePositionAndScale();
            SSTUModInterop.onPartGeometryUpdate(part, true);
        }

        private void updateTechLimits()
        {
            TechLimitHeightDiameter.updateTechLimits(techLimits, out techLimitMaxHeight, out techLimitMaxDiameter);

            if (currentTopDiameter > techLimitMaxDiameter)
            {
                currentTopDiameter = techLimitMaxDiameter;
            }
            if (currentBottomDiameter > techLimitMaxDiameter)
            {
                currentBottomDiameter = techLimitMaxDiameter;
            }
            if (currentHeight > techLimitMaxHeight)
            {
                currentHeight = techLimitMaxHeight;
            }
        }

        private void updateEnginePositionAndScale()
        {
            float newScale = getEngineScale();
            float yPos = -(currentHeight * 0.5f) + (newScale * engineVerticalOffset);
            foreach(InterstageDecouplerEngine engine in engineModels)
            {
                engine.reposition(newScale, currentBottomDiameter * 0.5f, yPos, invertEngines);
            }
        }

        private void updateResources()
        {
            float scale = Mathf.Pow(getEngineScale(), thrustScalePower);
            float volume = resourceVolume * scale * numberOfEngines;
            if (useRF)
            {
                SSTUModInterop.onPartFuelVolumeUpdate(part, volume);
            }
            else
            {
                print("setting solid fuel quantity based on resource volume: " + volume);
                SSTUResourceList resources = fuelType.getResourceList(volume);
                resources.setResourcesToPart(part, HighLogic.LoadedSceneIsEditor);
            }
        }

        private void updatePartMass()
        {
            //TODO
        }

        private void updateEngineThrust()
        {
            ModuleEngines engine = part.GetComponent<ModuleEngines>();
            if (engine != null)
            {
                float scale = getEngineScale();
                float thrustScalar = Mathf.Pow(scale, thrustScalePower);
                float thrustPerEngine = engineThrust * thrustScalar;
                float totalThrust = thrustPerEngine * numberOfEngines;
                guiEngineThrust = totalThrust;
                SSTUUtils.updateEngineThrust(engine, engine.minThrust, totalThrust);
            }
            else
            {
                print("Cannot update engine thrust -- no engine module found!");
                guiEngineThrust = 0;
            }
        }

        private void updateNodePositions(bool userInput)
        {
            SSTUAttachNodeUtils.updateAttachNodePosition(part, part.findAttachNode("top"), new Vector3(0, currentHeight * 0.5f, 0), Vector3.up, userInput);
            SSTUAttachNodeUtils.updateAttachNodePosition(part, part.findAttachNode("bottom"), new Vector3(0, -currentHeight * 0.5f, 0), Vector3.down, userInput);
        }

        private float getEngineScale()
        {
            return currentBottomDiameter / defaultModelScale;
        }

        private void updateGuiFields()
        {
            guiTaperHeight = currentTaperHeight;
            guiTopDiameter = currentTopDiameter;
            guiBottomDiameter = currentBottomDiameter;
        }

        private class InterstageDecouplerEngine
        {
            Transform model;
            float angle;
            float offsetAngle;

            public InterstageDecouplerEngine(Transform model, float placementAngle, float rotationOffset)
            {
                this.model = model;
                this.angle = placementAngle;
                this.offsetAngle = rotationOffset;
            }
            
            public void reposition(float scale, float radius, float yPos, bool invert)
            {
                model.transform.rotation = model.transform.parent.rotation;
                float x = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                float z = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                model.transform.localPosition = new Vector3(x, yPos, z);
                model.Rotate(Vector3.up, angle + offsetAngle, Space.Self);
                if (invert)
                {
                    model.Rotate(Vector3.left, 180f, Space.Self);//x-axis is default for rcs blocks -- TODO add config to use other axis
                }
                model.transform.localScale = new Vector3(scale, scale, scale);
            }            
        }

        private class InterstageDecouplerModel : FairingContainer
        {
            private GameObject collider;
            private float colliderHeight;
            
            public InterstageDecouplerModel(GameObject root, GameObject collider, float colliderHeight, int cylinderFaces, int numberOfPanels, float thickness) : base(root, cylinderFaces, numberOfPanels, thickness)
            {
                this.collider = collider;
                this.colliderHeight = colliderHeight;
            }

            public override void generateFairing()
            {
                base.generateFairing();
                rebuildCollider();
            }

            //TODO
            private void rebuildCollider()
            {
                //throw new NotImplementedException();
            }
        }

    }
}
