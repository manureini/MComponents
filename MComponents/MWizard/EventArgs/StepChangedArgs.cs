namespace MComponents.MWizard
{
    public class StepChangedArgs : ICancelableEvent
    {
        public int OldStepIndex { get; set; }

        public int NewStepIndex { get; set; }

        public MWizardStep OldStep { get; set; }

        public MWizardStep NewStep { get; set; }

        public bool UserInteract { get; set; }

        public bool Cancelled { get; set; }
    }
}
