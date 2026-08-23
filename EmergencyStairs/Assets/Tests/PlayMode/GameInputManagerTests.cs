using NUnit.Framework;
using UnityEngine;

public class GameInputManagerTests
{
    private GameInputManager sut;

    [SetUp]
    public void SetUp()
    {
        sut = new GameObject(nameof(GameInputManagerTests)).AddComponent<GameInputManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (sut != null)
            Object.DestroyImmediate(sut.gameObject);
    }

    [Test]
    public void InitiallyGameplayInputIsEnabled()
    {
        Assert.IsTrue(sut.IsGameplayInputEnabled);
    }

    [Test]
    public void RequestUIMode_DisablesGameplayInput()
    {
        sut.RequestUIMode(this);

        Assert.IsFalse(sut.IsGameplayInputEnabled);
    }

    [Test]
    public void ReleaseUIMode_ReEnablesGameplayInput_OnlyWhenAllRequestersReleased()
    {
        var requesterA = new object();
        var requesterB = new object();

        sut.RequestUIMode(requesterA);
        sut.RequestUIMode(requesterB);
        Assert.IsFalse(sut.IsGameplayInputEnabled, "should stay disabled while any requester is active");

        sut.ReleaseUIMode(requesterA);
        Assert.IsFalse(sut.IsGameplayInputEnabled, "should still be disabled while requesterB remains");

        sut.ReleaseUIMode(requesterB);
        Assert.IsTrue(sut.IsGameplayInputEnabled, "should re-enable once every requester has released");
    }

    [Test]
    public void GameplayInputEnabled_ReactivePropertyReflectsCurrentState()
    {
        Assert.IsTrue(sut.GameplayInputEnabled.Value);

        sut.RequestUIMode(this);

        Assert.IsFalse(sut.GameplayInputEnabled.Value);
    }
}
