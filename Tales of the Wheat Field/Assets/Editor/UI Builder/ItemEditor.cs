using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor.UIElements; // 引入ObjectField所在的命名空间



public class ItemEditor : EditorWindow
{

    private ItemDataList_SO dataBase;
    /// <summary>
    /// 列表信息
    /// </summary>
    private List<ItemDetails> itemList=new List<ItemDetails>();
    private VisualTreeAsset itemRowTemplate;
    private ScrollView itemDetailsSection;
    private ItemDetails activeItem;
    //获取VisualElement
    private ListView itemListView;

    private VisualElement iconPreview;
    private Sprite defaultIcon;


    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("M STUDIO/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        //VisualElement label = new Label("Hello World! From C#");
        //root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        //拿到我们的模板数据
        itemRowTemplate=AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemRowTemplate.uxml");

        //变量赋值,拿到左侧列表
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        itemDetailsSection = root.Q<ScrollView>("ItemDetails");
     

        //获取默认Icon图片
        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_M.png");
        iconPreview = itemDetailsSection.Q<VisualElement>("Icon");

        //获得按钮
        root.Q<Button>("AddButton").clicked += OnAddItemClicked;
        root.Q<Button>("DeleteButton").clicked += OnDeleteClicked;

        //加载数据
        LoadDataBase();

        //生成ListView
        GenerateListView();
    }

    private void OnDeleteClicked()
    {
        itemList.Remove(activeItem);
        itemListView.Rebuild();

        itemDetailsSection.visible = false;
    }

    private void OnAddItemClicked()
    {
        ItemDetails newItem= new ItemDetails();
        newItem.itemName = "NEW ITEM";
        newItem .itemID=1001+itemList.Count;

        itemList.Add(newItem);
        //刷新
        itemListView.Rebuild();
    }
    /// <summary>
    /// 获取ItemDataList_SO数据
    /// </summary>
    private void LoadDataBase()
    {
        //获取 Assets目录下的ItemDataList_SO名称的GUID（唯一标识符）
        var dataArray = AssetDatabase.FindAssets("ItemDataList_SO");
        //检测是否获得GUID
        if (dataArray.Length > 1)
        {
            //将获取的第一个GUID转化为path（地址）
            var path=AssetDatabase.GUIDToAssetPath(dataArray[0]);
            //依据路径加载资产，并将其转换为ItemDataList_SO类型后赋值给dataBase变量
            dataBase = AssetDatabase.LoadAssetAtPath(path,typeof(ItemDataList_SO)) as ItemDataList_SO;  
        }
        
        itemList = dataBase.ItemDetailsList;
        //告诉 Unity 编辑器某个资源已被修改，需要在保存项目时持久化这些变更。
        EditorUtility.SetDirty(dataBase);
    
        
    }
    /// <summary>
    /// 列表显示数据
    /// </summary>
    private void GenerateListView()
    {
        //复制一个模板
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();
        //bindItem是连接数据和 UI 的桥梁，它定义了 “如何将数据展示在界面上”
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if (i < itemList.Count)
            {
                if (itemList[i].itemIcon != null)
                    //从数据源中获取第i个物品的图标纹理
                    e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIcon.texture;
                e.Q<Label>("Name").text = itemList[i]==null?"NO ITEM":itemList[i].itemName;
            }
        };
        itemListView.fixedItemHeight = 60;

        itemListView.itemsSource = itemList;
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;
        //为 ListView 组件注册了一个选择变更事件，用于监听用户在列表中选择项的变化
        itemListView.onSelectionChange += OnListSelectionChane;
        //设置右侧信息面板不可见
        itemDetailsSection.visible=false;

    }

    private void OnListSelectionChane(IEnumerable<object> selectedItem)
    {
        activeItem = (ItemDetails)selectedItem.First();
        GetItemDetails();
        itemDetailsSection.visible = true;
    }

    private void GetItemDetails()
    {
        //
        itemDetailsSection.MarkDirtyRepaint(); 


        //在详细面板中显示id
        itemDetailsSection.Q<IntegerField>("ItemID").value=activeItem.itemID;
        //回调函数，当详细面板中的值被更改时，database也会更改
        itemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemID = evt.newValue;
        });

        itemDetailsSection.Q<TextField>("ItemName").value = activeItem.itemName;
        itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemName = evt.newValue;
            //重新刷新itemListView，更新数据
            itemListView.Rebuild();
        });

        iconPreview.style.backgroundImage = activeItem.itemIcon == null ? defaultIcon.texture : activeItem.itemIcon.texture;
        itemDetailsSection.Q<UnityEditor.UIElements.ObjectField>("ItemIcon").value = activeItem.itemIcon;
        itemDetailsSection.Q<UnityEditor.UIElements.ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            Sprite newIcon = evt.newValue as Sprite;
            activeItem.itemIcon = newIcon;

            iconPreview.style.backgroundImage = newIcon == null ? defaultIcon.texture : newIcon.texture;
            itemListView.Rebuild();
        });



        // 其他所有变量的绑定
        itemDetailsSection.Q<UnityEditor.UIElements.ObjectField>("ItemSprite").value = activeItem.itemOnWorldSprite;
        itemDetailsSection.Q<UnityEditor.UIElements.ObjectField>("ItemSprite").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemOnWorldSprite = (Sprite)evt.newValue;
        });

        itemDetailsSection.Q<EnumField>("ItemType").Init(activeItem.itemType);
        itemDetailsSection.Q<EnumField>("ItemType").value = activeItem.itemType;
        itemDetailsSection.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemType = (ItemType)evt.newValue;
        });

        itemDetailsSection.Q<TextField>("Description").value = activeItem.itemDescription;
        itemDetailsSection.Q<TextField>("Description").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemDescription = evt.newValue;
        });

        itemDetailsSection.Q<IntegerField>("ItemUseRadius").value = activeItem.itemUseRadius;
        itemDetailsSection.Q<IntegerField>("ItemUseRadius").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemUseRadius = evt.newValue;
        });

        itemDetailsSection.Q<Toggle>("CanPickedup").value = activeItem.canPickedup;
        itemDetailsSection.Q<Toggle>("CanPickedup").RegisterValueChangedCallback(evt =>
        {
            activeItem.canPickedup = evt.newValue;
        });

        itemDetailsSection.Q<Toggle>("CanDropped").value = activeItem.canDropped;
        itemDetailsSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt =>
        {
            activeItem.canDropped = evt.newValue;
        });

        itemDetailsSection.Q<Toggle>("CanCarried").value = activeItem.canCarried;
        itemDetailsSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback(evt =>
        {
            activeItem.canCarried = evt.newValue;
        });
       

        itemDetailsSection.Q<IntegerField>("Price").value = activeItem.itemPrice;
        itemDetailsSection.Q<IntegerField>("Price").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemPrice = evt.newValue;
        });

        itemDetailsSection.Q<Slider>("SellPercentage").value = activeItem.sellPercentage;
        itemDetailsSection.Q<Slider>("SellPercentage").RegisterValueChangedCallback(evt =>
        {
            activeItem.sellPercentage = evt.newValue;
        });

        //Row4
        itemDetailsSection.Q<Toggle>("CanEffect").value = activeItem.canEffect;
        itemDetailsSection.Q<Toggle>("CanEffect").RegisterValueChangedCallback(evt =>
        {
            activeItem.canEffect = evt.newValue;
        });

        itemDetailsSection.Q<EnumField>("attribute").Init(activeItem.attribute);
        itemDetailsSection.Q<EnumField>("attribute").value = activeItem.attribute;
        itemDetailsSection.Q<EnumField>("attribute").RegisterValueChangedCallback(evt =>
        {
            activeItem.attribute = (Attribute)evt.newValue;
        });


        //在详细面板中显示id
        itemDetailsSection.Q<IntegerField>("AddValue").value = activeItem.AddValue;
        //回调函数，当详细面板中的值被更改时，database也会更改
        itemDetailsSection.Q<IntegerField>("AddValue").RegisterValueChangedCallback(evt =>
        {
            activeItem.AddValue = evt.newValue;
        });

        //在详细面板中显示id
        itemDetailsSection.Q<FloatField>("timeSize").value = activeItem.timeSize;
        //回调函数，当详细面板中的值被更改时，database也会更改
        itemDetailsSection.Q<FloatField>("timeSize").RegisterValueChangedCallback(evt =>
        {
            activeItem.timeSize = evt.newValue;
        });

        itemDetailsSection.Q<Toggle>("isIncrease").value = activeItem.isIncrease;
        itemDetailsSection.Q<Toggle>("isIncrease").RegisterValueChangedCallback(evt =>
        {
            activeItem.isIncrease = evt.newValue;
        });

        itemDetailsSection.Q<Toggle>("istimeLimit").value = activeItem.istimeLimit;
        itemDetailsSection.Q<Toggle>("istimeLimit").RegisterValueChangedCallback(evt =>
        {
            activeItem.istimeLimit = evt.newValue;
        });
    }
}
