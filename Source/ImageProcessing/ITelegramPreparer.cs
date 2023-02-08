namespace ImoutoPicsBot.ImageProcessing;

public interface ITelegramPreparer
{
    Stream Prepare(Stream input, long inputByteLength);
}
