using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class AlignPivot : MonoBehaviour
{
	private const string COLLIDER_NAME = "_Collider";
	private const string NAVMESHOBSTACLE_NAME = "_NavMeshObstacle";
    
	private const string UNDO_ALIGNPIVOT = "_AlignPivot";

	private static bool makeColliObjPivot = false;
	private static bool makeNaMeObsObjPivot = false;

	[MenuItem("GameObject/Align Pivot to Center %m")]
    public static void AlignCenterPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Center));
    
    [MenuItem("GameObject/Align Pivot SomeOne/Up")]
    public static void AlignUpPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Up));
    
    [MenuItem("GameObject/Align Pivot SomeOne/Down")]
    public static void AlignDownPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Down));
    
    [MenuItem("GameObject/Align Pivot SomeOne/Left")]
    public static void AlignLeftPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Left));
    
    [MenuItem("GameObject/Align Pivot SomeOne/Right")]
    public static void AlignRightPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Right));
    
    [MenuItem("GameObject/Align Pivot SomeOne/Front")]
    public static void AlignFrontPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Front));
    
    [MenuItem("GameObject/Align Pivot SomeOne/Back")]
    public static void AlignBackPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Back));
    
    [MenuItem("GameObject/Align Pivot SomeOne/Min")]
    public static void AlignMinPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Min));
    
    [MenuItem("GameObject/Align Pivot SomeOne/Max")]
    public static void AlignMaxPivot() => SetPivot(Selection.activeTransform,PosReturn(Selection.activeTransform,PosStand.Max));

    private Vector3 PosReturn(Transform target, string posStand)
    {
        Enum.TryParse(posStand, out PosStand stand);
        return PosReturn(target, stand);
    }

    private static Vector3 PosReturn(Transform target,PosStand posStand)
    {
        //非アクティブ含めtargetとtargetの子オブジェクト全てのrendererとcolliderを取得
        var renderers = target.GetComponentsInChildren<Renderer>(true);
        var colliders = target.GetComponentsInChildren<Collider>(true);
        
        //rendererとcolliderもない場合そのまま座標を返す
        if(renderers.Length==0&&colliders.Length==0){ return target.position;}
        
        Bounds bounds = default;
        var once = false;
        
        foreach (var renderer in renderers)
        {
            if (!once)
            {
                bounds = renderer.bounds;
                once = true;
                continue;
            }
            
            bounds.Encapsulate(renderer.bounds);
        }
        
        foreach (var collider in colliders)
        {
            if (!once)
            {
                bounds = collider.bounds;
                once = true;
                continue;
            }
            
            bounds.Encapsulate(collider.bounds);
        }

        return posStand switch
        {
            PosStand.Center => bounds.center,
            PosStand.Up => bounds.center+new Vector3(0,bounds.extents.y,0),
            PosStand.Down=> bounds.center+new Vector3(0,-bounds.extents.y,0),
            PosStand.Left => bounds.center+new Vector3(-bounds.extents.x,0,0),
            PosStand.Right => bounds.center+new Vector3(bounds.extents.x,0,0),
            PosStand.Front => bounds.center+new Vector3(0,0,-bounds.extents.z),
            PosStand.Back => bounds.center+new Vector3(0,0,bounds.extents.z),
            PosStand.Min => bounds.min,
            PosStand.Max => bounds.max,
            _ => throw new ArgumentOutOfRangeException(nameof(posStand), posStand, null)
        };
    }

    private static void SetPivot( Transform target,Vector3 pivotPos)
	{
		//meshFilter取得
		var meshFilter = target.GetComponent<MeshFilter>();
		//オリジナルのメッシュ
		var originalMesh = (meshFilter) ? meshFilter.sharedMesh : null;
	    
		//meshFilterがあったら
		if (meshFilter!=null)
		{
			//RecordObject が呼び出された後の変更点を記録
			Undo.RecordObject( meshFilter, UNDO_ALIGNPIVOT );
		    
			//メッシュを複製
			var meshCopy = Instantiate(originalMesh);
		    
			//メッシュをいれかえる
			meshFilter.sharedMesh = meshCopy;
		    
			//頂点の位置を取得
			var vertices = meshCopy.vertices;
		    
			if( pivotPos != Vector3.zero )
			{
				//各頂点の位置からpivotPos座標を引く
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i] -= pivotPos;
				}

				meshCopy.vertices = vertices;
			    
				//頂点からメッシュのバウンディングボリュームを再計算
				meshCopy.RecalculateBounds();
			}
		}
	    
		makeColliObjPivot = EditorPrefs.GetBool( "AlignPivotMakeCollis", false );
		makeNaMeObsObjPivot = EditorPrefs.GetBool( "AlignPivotMakeNaMeObs", false );
	    
		//targetのみのコライダー全取得
		var colliders = target.GetComponents<Collider>();
		foreach (var collider in colliders)
		{
			var meshCollider = (MeshCollider) collider;

			if (meshCollider != null && originalMesh != null && meshCollider.sharedMesh == originalMesh)
			{
				Undo.RecordObject( meshCollider, UNDO_ALIGNPIVOT );
				meshCollider.sharedMesh = meshFilter.sharedMesh;
			}
		}

		if( makeColliObjPivot && target.Find( COLLIDER_NAME )==null )
		{
			GameObject colliderObj = null;
			foreach( var collider in colliders )
			{
				if(collider==null) { continue; }

				var meshCollider = (MeshCollider)collider;
				if (meshCollider != null && meshCollider.sharedMesh == meshFilter.sharedMesh) { continue; }
				if( colliderObj == null )
				{
					colliderObj = new GameObject( COLLIDER_NAME );
					colliderObj.transform.SetParent( target, false );
				}

				EditorUtility.CopySerialized( collider, colliderObj.AddComponent( collider.GetType() ) );
			}

			if (colliderObj != null)
			{
				Undo.RegisterCreatedObjectUndo( colliderObj, UNDO_ALIGNPIVOT ); 
			}
		}

		if( makeNaMeObsObjPivot && target.Find( NAVMESHOBSTACLE_NAME )==null )
		{
			var navMeshObstacle = target.GetComponent<NavMeshObstacle>();
			if(navMeshObstacle!=null)
			{
				var navMeObsObj = new GameObject( NAVMESHOBSTACLE_NAME );
				navMeObsObj.transform.SetParent( target, false );
				EditorUtility.CopySerialized( navMeshObstacle, navMeObsObj.AddComponent( navMeshObstacle.GetType() ) );
				Undo.RegisterCreatedObjectUndo( navMeObsObj, UNDO_ALIGNPIVOT );
			}
		}

		var children = new Transform[target.childCount];
		var childrenPoss = new Vector3[children.Length];
		var childrenRotes = new Quaternion[children.Length];
		
		for( int i = children.Length - 1; i >= 0; i-- )
		{
			children[i] = target.GetChild( i );
			childrenPoss[i] = children[i].position;
			childrenRotes[i] = children[i].rotation;

			Undo.RecordObject( children[i], UNDO_ALIGNPIVOT );
		}

		Undo.RecordObject( target, UNDO_ALIGNPIVOT );
		target.position = pivotPos;

		for( int i = 0; i < children.Length; i++ )
		{
			children[i].position = childrenPoss[i];
			children[i].rotation = childrenRotes[i];
		}
	}
    
    //どの立ち位置の座標を吐き出すか
    private enum PosStand{ Center, Up, Down, Left, Right, Front, Back, Min, Max }
}
