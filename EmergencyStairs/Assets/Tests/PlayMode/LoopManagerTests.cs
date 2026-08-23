using NUnit.Framework;
using UnityEngine;

public class LoopManagerTests
{
    private LoopManager sut;

    [SetUp]
    public void SetUp()
    {
        sut = new GameObject(nameof(LoopManagerTests)).AddComponent<LoopManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (sut != null)
            Object.DestroyImmediate(sut.gameObject);
    }

    [Test]
    public void InitialLoopIsZero()
    {
        Assert.AreEqual(0, sut.CurrentLoop);
    }

    [Test]
    public void AdvanceLoop_IncrementsCurrentLoop()
    {
        sut.AdvanceLoop();
        sut.AdvanceLoop();

        Assert.AreEqual(2, sut.CurrentLoop);
    }

    [Test]
    public void ResetLoops_ReturnsToZeroAndClearsFlags()
    {
        sut.AdvanceLoop();
        sut.SetFlag("door_unlocked");

        sut.ResetLoops();

        Assert.AreEqual(0, sut.CurrentLoop);
        Assert.IsFalse(sut.HasFlag("door_unlocked"));
    }

    [Test]
    public void SetFlag_ClearFlag_HasFlag_RoundTrip()
    {
        Assert.IsFalse(sut.HasFlag("phone_answered"));

        sut.SetFlag("phone_answered");
        Assert.IsTrue(sut.HasFlag("phone_answered"));

        sut.ClearFlag("phone_answered");
        Assert.IsFalse(sut.HasFlag("phone_answered"));
    }

    [Test]
    public void CurrentLoopRP_ReflectsCurrentLoop()
    {
        sut.AdvanceLoop();

        Assert.AreEqual(sut.CurrentLoop, sut.CurrentLoopRP.Value);
    }
}
