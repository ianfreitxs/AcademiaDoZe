// Ian Freitas de Sousa
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Message;

public sealed class TemaPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value)
{
}