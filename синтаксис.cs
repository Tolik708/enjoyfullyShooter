//основные и самый чато забываемый синтаксис

//найти обэкт с определенным именем в другом обэкте
SearchingObj = ParentObj.Find("nameOfSearchingObj");

//найто обект в Hierarchy
GameObject.Find("test");
// найти без рдителя или с определенным родителем
GameObject.Find("/test");

//Instantiate
public GameObject prefab;
public Transform position;
Instantiate(prefab, position.position, Quaternion.identity);

//Quaternion
Quaternion.identity;
new Quaternion(x, y, z, w);

///// textMeshPro
using TMPro
TextMeshPro testText;
testText.text = "test" //Not UI

TextMeshProUGUI testText1;
testText1.text = "test" //For UI


//collision Enter/Exit/Stay
void OnCollisionEnter2D(Collision2D col)
{
	Debug.Log(col.gameObject.name);
}

//Coroutines
StartCoroutine(test());
IEnumerator test()
{
	// wait comand
	yield return new WaitForSeconds(time);
}

//raycast
Physics.Raycast(position, dir, out hit, MaxDist, Layer);

//SphereCast
Physics.SphereCast(position, radius, dir, out hit, MaxDist, Layer);

//center of object
transform.renderer.bounds.center;

//find dir
Vector3 dir = destination - origin;

//compare tag
other = collision;
other.CompareTag("Player");