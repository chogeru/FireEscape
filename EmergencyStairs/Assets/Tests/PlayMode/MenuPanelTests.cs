using NUnit.Framework;
using UnityEngine;

public class MenuPanelTests
{
    private GameObject go;
    private MenuPanel sut;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject(nameof(MenuPanelTests));
        sut = go.AddComponent<MenuPanel>();
    }

    [TearDown]
    public void TearDown()
    {
        if (go != null)
            Object.DestroyImmediate(go);
    }

    [Test]
    public void Open_ActivatesRootAndSetsIsOpenTrue()
    {
        go.SetActive(false);

        sut.Open();

        Assert.IsTrue(go.activeSelf);
        Assert.IsTrue(sut.IsOpen.Value);
    }

    [Test]
    public void Close_DeactivatesRootAndSetsIsOpenFalse()
    {
        sut.Open();

        sut.Close();

        Assert.IsFalse(go.activeSelf);
        Assert.IsFalse(sut.IsOpen.Value);
    }
}
