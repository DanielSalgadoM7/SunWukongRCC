using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "") {
            Debug.Log(fieldName + " tá vazio, precisa de um objeto " + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    //a gente vai conferir se a lista de salas ta vazia
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck){
        bool error = false;
        int count = 0;

        foreach(var item in enumerableObjectToCheck){
            if(item == null){
                Debug.Log(fieldName + " objeto tem valor nulo " + thisObject.name.ToString());
                error = true;
            }
            else {
                count++;
            }
        }

        if(count == 0){
            Debug.Log(fieldName + " não tem valor no objeto " + thisObject.name.ToString());
            error = true;
        }
        return error;
    }
}
