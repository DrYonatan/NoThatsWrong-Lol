using System;

public interface IHubPanel
{
    void Show(Action onReturnToHub);
    void Hide();
}
