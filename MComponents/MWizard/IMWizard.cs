namespace MComponents.MWizard
{
    public interface IMWizard
    {
        void RegisterStep(MWizardStep pStep);

        void InvokeStateHasChanged();
    }
}
