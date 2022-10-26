using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Renderer;
using BlazorStudio.ClassLib.Store.NotificationCase;
using Fluxor;

namespace BlazorStudio.ClassLib.Store.FileSystemCase;

public class FileSystemStateEffects
{
    private readonly IDefaultErrorRenderer _defaultErrorRenderer;
    private readonly IDefaultInformationRenderer _defaultInformationRenderer;
    private readonly IFileSystemProvider _fileSystemProvider;

    private readonly Dictionary<AbsoluteFilePathStringValue, (Task writeTask, string content)>
        _trackFileSystemWritesMap = new();

    private readonly SemaphoreSlim _trackFileSystemWritesMapSemaphoreSlim = new(1, 1);

    public FileSystemStateEffects(IFileSystemProvider fileSystemProvider,
        IDefaultInformationRenderer defaultInformationRenderer,
        IDefaultErrorRenderer defaultErrorRenderer)
    {
        _fileSystemProvider = fileSystemProvider;
        _defaultInformationRenderer = defaultInformationRenderer;
        _defaultErrorRenderer = defaultErrorRenderer;
    }

    /// <summary>
    ///     If <see cref="_trackFileSystemWritesMap" /> has an entry with the
    ///     <see cref="WriteToFileSystemAction.AbsoluteFilePath" />
    ///     then check if the entry's <see cref="Task" /> has completed.
    ///     <br /><br />
    ///     If a previous write task to the same physical file was already in <see cref="_trackFileSystemWritesMap" />
    ///     and the task has completed. Then check if the string content written out is equal. If the content being
    ///     written is equal then do nothing.
    ///     <br /><br />
    ///     If <see cref="_trackFileSystemWritesMap" /> shows that the file content is different and needs updating then
    ///     proceed with setting the new value for the Key provided. Further write requests are to await the previous write
    ///     request to complete.
    ///     <br /><br />
    ///     <see cref="_trackFileSystemWritesMapSemaphoreSlim" /> is used to ensure when a write request comes in
    ///     that while the first write request is being validated a second write request cannot come along and for whatever
    ///     reason
    ///     finish being validated before the first write request was causing a de-sync.
    ///     <br /><br />
    ///     Validated in this context refers to: (does the file actually need updated or is there an in progress write task
    ///     that needs awaiting)
    /// </summary>
    [EffectMethod]
    public async Task ReduceWriteToFileSystemAction(WriteToFileSystemAction writeToFileSystemAction,
        IDispatcher dispatcher)
    {
        try
        {
            await _trackFileSystemWritesMapSemaphoreSlim.WaitAsync();

            var absoluteFilePathStringValue =
                new AbsoluteFilePathStringValue(writeToFileSystemAction.AbsoluteFilePath);

            if (_trackFileSystemWritesMap.TryGetValue(absoluteFilePathStringValue, out var previousWriteTask))
            {
                if (previousWriteTask.content == writeToFileSystemAction.Content)
                {
                    dispatcher.Dispatch(new RegisterNotificationAction(new NotificationRecord(
                        NotificationKey.NewNotificationKey(),
                        $"No changes to write out: {writeToFileSystemAction.AbsoluteFilePath.GetAbsoluteFilePathString()}",
                        _defaultErrorRenderer.GetType(),
                        null,
                        TimeSpan.FromSeconds(3))));

                    return;
                }

                await previousWriteTask.writeTask;
            }

            var writeTask = Task.Run(async () =>
            {
                await _fileSystemProvider.WriteFileAsync(
                    writeToFileSystemAction.AbsoluteFilePath,
                    writeToFileSystemAction.Content,
                    false,
                    false,
                    CancellationToken.None);
            });

            if (previousWriteTask != default)
            {
                _trackFileSystemWritesMap[absoluteFilePathStringValue] =
                    (writeTask, writeToFileSystemAction.Content);
            }
            else
            {
                _trackFileSystemWritesMap.Add(
                    absoluteFilePathStringValue,
                    (writeTask, writeToFileSystemAction.Content));
            }

            dispatcher.Dispatch(new RegisterNotificationAction(new NotificationRecord(
                NotificationKey.NewNotificationKey(),
                $"Saved file: {writeToFileSystemAction.AbsoluteFilePath.GetAbsoluteFilePathString()}",
                _defaultInformationRenderer.GetType(),
                null,
                TimeSpan.FromSeconds(3))));
        }
        finally
        {
            _trackFileSystemWritesMapSemaphoreSlim.Release();
        }
    }
}