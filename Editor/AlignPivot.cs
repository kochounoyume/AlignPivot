using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class AlignPivot : MonoBehaviour
{
	private const string GENERATED_COLLIDER_NAME = "__GeneratedCollider";
	private const string GENERATED_NAVMESH_OBSTACLE_NAME = "__GeneratedNavMeshObstacle";
    
	private const string UNDO_ADJUST_PIVOT = "Move Pivot";

	private static bool createColliderObjectOnPivotChange = false;
	private static bool createNavMeshObstacleObjectOnPivotChange = false;

	private GUIStyle buttonStyle;
	private GUIStyle headerStyle;

	private Vector3 selectionPrevPos;
	private Vector3 selectionPrevRot;
	
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

    private static void GetPrefs()
    {
	    createColliderObjectOnPivotChange = EditorPrefs.GetBool( "AdjustPivotCreateColliders", false );
	    createNavMeshObstacleObjectOnPivotChange = EditorPrefs.GetBool( "AdjustPivotCreateNavMeshObstacle", false );
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
			Undo.RecordObject( meshFilter, UNDO_ADJUST_PIVOT );
		    
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
	    
		GetPrefs();
	    
		//targetのみのコライダー全取得
		var colliders = target.GetComponents<Collider>();
		foreach (var collider in colliders)
		{
			var meshCollider = (MeshCollider) collider;

			if (meshCollider != null && originalMesh != null && meshCollider.sharedMesh == originalMesh)
			{
				Undo.RecordObject( meshCollider, UNDO_ADJUST_PIVOT );
				meshCollider.sharedMesh = meshFilter.sharedMesh;
			}
		}

		if( createColliderObjectOnPivotChange && target.Find( GENERATED_COLLIDER_NAME )==null )
		{
			GameObject colliderObj = null;
			foreach( var collider in colliders )
			{
				if(collider==null) { continue; }

				var meshCollider = (MeshCollider)collider;
				if (meshCollider != null && meshCollider.sharedMesh == meshFilter.sharedMesh) { continue; }
				if( colliderObj == null )
				{
					colliderObj = new GameObject( GENERATED_COLLIDER_NAME );
					colliderObj.transform.SetParent( target, false );
				}

				EditorUtility.CopySerialized( collider, colliderObj.AddComponent( collider.GetType() ) );
			}

			if (colliderObj != null)
			{
				Undo.RegisterCreatedObjectUndo( colliderObj, UNDO_ADJUST_PIVOT ); 
			}
		}

		if( createNavMeshObstacleObjectOnPivotChange && target.Find( GENERATED_NAVMESH_OBSTACLE_NAME )==null )
		{
			var obstacle = target.GetComponent<NavMeshObstacle>();
			if(obstacle!=null)
			{
				var obstacleObj = new GameObject( GENERATED_NAVMESH_OBSTACLE_NAME );
				obstacleObj.transform.SetParent( target, false );
				EditorUtility.CopySerialized( obstacle, obstacleObj.AddComponent( obstacle.GetType() ) );
				Undo.RegisterCreatedObjectUndo( obstacleObj, UNDO_ADJUST_PIVOT );
			}
		}

		var children = new Transform[target.childCount];
		var childrenPositions = new Vector3[children.Length];
		var childrenRotations = new Quaternion[children.Length];
		
		for( int i = children.Length - 1; i >= 0; i-- )
		{
			children[i] = target.GetChild( i );
			childrenPositions[i] = children[i].position;
			childrenRotations[i] = children[i].rotation;

			Undo.RecordObject( children[i], UNDO_ADJUST_PIVOT );
		}

		Undo.RecordObject( target, UNDO_ADJUST_PIVOT );
		target.position = pivotPos;

		for( int i = 0; i < children.Length; i++ )
		{
			children[i].position = childrenPositions[i];
			children[i].rotation = childrenRotations[i];
		}
	}
    
    //どの立ち位置の座標を吐き出すか
    private enum PosStand
    {
        Center,
        Up,
        Down,
        Left,
        Right,
        Front,
        Back,
        Min,
        Max
    }
}
