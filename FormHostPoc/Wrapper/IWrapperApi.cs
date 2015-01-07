namespace FormHostPoc.Wrapper
{
    interface IWrapperApi
    {
        int OpenExe(string fileLocation, bool hasWrapper = true);
        bool CloseExe(int processId);

    }
}
