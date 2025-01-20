using UnityEngine;

public class RecognitionResult
{
    public const string AlternativesKey = "alternatives";
    public const string ResultKey = "result";
    public const string PartialKey = "partial";

    public RecognizedPhrase[] Phrases;
    public bool Partial;

    public RecognitionResult(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Phrases = new RecognizedPhrase[] { new RecognizedPhrase() { } };
            Debug.LogWarning("RecognitionResult: JSON is null or empty.");
            return;
        }

        JSONObject resultJson = JSONNode.Parse(json)?.AsObject;

        if (resultJson == null)
        {
            Phrases = new RecognizedPhrase[] { new RecognizedPhrase() { } };
            Debug.LogWarning("RecognitionResult: Failed to parse JSON.");
            return;
        }

        if (resultJson.HasKey(AlternativesKey))
        {
            var alternatives = resultJson[AlternativesKey].AsArray;
            Phrases = new RecognizedPhrase[alternatives.Count];

            for (int i = 0; i < Phrases.Length; i++)
            {
                Phrases[i] = new RecognizedPhrase(alternatives[i].AsObject);
                Debug.Log($"RecognitionResult: Alternative {i} - Text: {Phrases[i].Text}, Confidence: {Phrases[i].Confidence}");
            }
        }
        else if (resultJson.HasKey(ResultKey))
        {
            Phrases = new RecognizedPhrase[] { new RecognizedPhrase(resultJson.AsObject) };
            Debug.Log($"RecognitionResult: Single Result - Text: {Phrases[0].Text}, Confidence: {Phrases[0].Confidence}");
        }
        else if (resultJson.HasKey(PartialKey))
        {
            Partial = true;
            Phrases = new RecognizedPhrase[] { new RecognizedPhrase() { Text = resultJson[PartialKey] } };
            Debug.Log($"RecognitionResult: Partial Result - Text: {Phrases[0].Text}");
        }
        else
        {
            Phrases = new[] { new RecognizedPhrase() { } };
            Debug.LogWarning("RecognitionResult: No valid key found in JSON.");
        }
    }

    public string GetBestPhrase()
    {
        if (Phrases == null || Phrases.Length == 0)
        {
            return string.Empty;
        }

        RecognizedPhrase bestPhrase = Phrases[0];

        for (int i = 1; i < Phrases.Length; i++)
        {
            if (Phrases[i].Confidence > bestPhrase.Confidence)
            {
                Debug.Log("pre modifica:"+ bestPhrase.Text);
                bestPhrase = Phrases[i];
                Debug.Log("post modifica:"+ bestPhrase.Text);
            }
        }
        return bestPhrase.Text;
    }
}

public class RecognizedPhrase
{
    public const string ConfidenceKey = "confidence";
    public const string TextKey = "text";

    public string Text = "";
    public float Confidence = 0.0f;

    public RecognizedPhrase()
    {
    }

    public RecognizedPhrase(JSONObject json)
    {
        if (json.HasKey(ConfidenceKey))
        {
            Confidence = json[ConfidenceKey].AsFloat;
        }

        if (json.HasKey(TextKey))
        {
            // Rimuove lo spazio extra all'inizio della stringa aggiunto da Vosk
            Text = json[TextKey].Value.Trim();
        }
    }
}
