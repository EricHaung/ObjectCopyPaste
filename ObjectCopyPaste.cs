using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Engine;

public static class CopyPaste
{
    private static Engine.Mode Mode = Engine.Mode.mode3D;
    private static Object[] cacheSelectedObjects = null;
    private static List<Object> ObjectList = null;

    public static void CopyObject()
    {
        InitCacheContenter();

        if (EngineSelection.objects.Length > 0)
        {
            foreach (Object targetObject in EngineSelection.objects)
            {
                GameObject obj = targetObject as GameObject;

                if (!IsModule(obj) && !IsSomeObjChild(obj))
                {
                    ObjectList.Add(targetObject);
                }
            }
            cacheSelectedObjects = ObjectList.ToArray();
            ObjectList.Clear();
        }
    }

    private static bool IsSomeObjChild(GameObject myObject)
    {
        SceneObjectTag tag = myObject.GetComponent<SceneObjectTag>();

        if (!string.IsNullOrEmpty(tag.objParentID))
        {
            foreach (Object targetObject in EngineSelection.objects)
            {
                GameObject parentObj = targetObject as GameObject;
                SceneObjectTag parentTag = parentObj.GetComponent<SceneObjectTag>();
                if (parentTag.objID == tag.objParentID)
                    return true;
            }
            GameObject parentObject = FindObject(tag.objParentID);
            if (parentObject != null)
                return IsSomeObjChild(parentObject);
        }
        return false;
    }

    private static GameObject FindObject(string id)
    {
        SceneObjectTag[] parentObj = MonoBehaviour.FindObjectsOfType<SceneObjectTag>();

        foreach (SceneObjectTag tag in parentObj)
        {
            if (tag.objID == id)
                return tag.gameObject;
        }
        return null;
    }

    private static bool IsModule(GameObject myObject)
    {
        bool isModule = false;

        if (myObject.GetComponent<ProjectModuleAction>() != null)
        {
            isModule = true;
        }
        return isModule;
    }

    private static void InitCacheContenter()
    {
        if (Mode != Engine.getCurrentMode())
        {
            Mode = Engine.getCurrentMode();
        }
        if (ObjectList == null)
            ObjectList = new List<Object>();
        cacheSelectedObjects = null;
    }

    private static void PasteObject(GameObject[] objArray, UnitData[] resArray)
    {
        if (cacheSelectedObjects != null && Mode == Engine.getCurrentMode())
        {
            List<Object> sourceObjects = new List<Object>();

            sourceObjects = MappingSourceData();
            DuplicateObjects(objArray, resArray, sourceObjects);
        }
    }

    private static List<Object> MappingSourceData()
    {
        List<Object> selectedObjects = new List<Object>();

        for (int i = 0; i < cacheSelectedObjects.Length; i++)
        {
            if (cacheSelectedObjects[i] != null)
            {
                selectedObjects.Add(cacheSelectedObjects[i]);
            }
        }
        return selectedObjects;
    }

    public static void DuplicateObjects(GameObject[] objArray, UnitData[] resArray, List<Object> sourceObjects)
    {
        for (int i = 0; i < sourceObjects.Count; ++i)
        {
            if (sourceObjects[i] != null)
            {
                GameObject originObject = sourceObjects[i] as GameObject;

                GameObject duplicateObject = MonoBehaviour.Instantiate(originObject, originObject.transform.position, originObject.transform.rotation, originObject.transform.parent);

                RenameObject(objArray, originObject, duplicateObject);
                RemoveDuplicateObjectBehaviour(duplicateObject);

                UnitData unitData = GetUnitData(objArray, resArray, originObject);
                SetUpDuplicateGameObject(originObject, duplicateObject);
                SetUpParent(originObject, duplicateObject);
                ReturnDuplicateObject(duplicateObject, unitData);
                GetChildObject(objArray, resArray, duplicateObject, originObject);
            }
        }
    }
}
