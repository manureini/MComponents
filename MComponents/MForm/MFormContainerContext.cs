using MComponents.Notifications;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MComponents.MForm
{
    public class MFormContainerContext
    {
        public delegate Task<bool> AsyncEventHandler<TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs;

        public event AsyncEventHandler<MFormContainerContextSubmitArgs> OnFormSubmit;

        protected SemaphoreSlim mLocker = new SemaphoreSlim(1, 1);

        public List<IMForm> Forms { get; set; } = new List<IMForm>();


        public MFormContainer FormContainer { get; protected set; }

        public IServiceProvider ServiceProvider { get; protected set; }

        internal MFormContainerContext(IServiceProvider pServiceProvider, MFormContainer pFormContainer)
        {
            ServiceProvider = pServiceProvider;
            FormContainer = pFormContainer;
        }

        public void RegisterForm(IMForm pForm)
        {
            Forms.Add(pForm);
        }

        public async Task<bool> NotifySubmit(IStringLocalizer pLocalizer)
        {
            if (OnFormSubmit == null)
                return true;

            bool submitSuccessful = true;

            try
            {
                await mLocker.WaitAsync();

                var args = new MFormContainerContextSubmitArgs()
                {
                    UserInterated = true
                };

                foreach (AsyncEventHandler<MFormContainerContextSubmitArgs> handler in OnFormSubmit.GetInvocationList())
                {
                    try
                    {
                        var formValid = await handler(this, args);

                        if (submitSuccessful && !formValid)
                        {
                            submitSuccessful = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in the handler {0}: {1}", handler.Method.Name, e.Message);

                        string msg = e.ToString();

                        if (e is TargetInvocationException te)
                        {
                            msg = te.InnerException.ToString();

                            if (te.InnerException is UserMessageException ue)
                                msg = ue.Message;
                        }

                        Notificator.InvokeNotification(ServiceProvider, true, msg);
                        submitSuccessful = false;
                    }
                }

                if (submitSuccessful)
                {
                    if (FormContainer.OnAfterAllFormsSubmitted.HasDelegate)
                    {
                        await FormContainer.OnAfterAllFormsSubmitted.InvokeAsync(new MFormContainerAfterAllFormsSubmittedArgs());
                    }

                    Notificator.InvokeNotification(ServiceProvider, false, pLocalizer["Gespeichert!"]);
                }
            }
            finally
            {
                mLocker.Release();
            }

            return submitSuccessful;
        }
    }
}
