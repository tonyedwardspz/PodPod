using System.Text.RegularExpressions;

namespace PodPod.Services;

public static class HTMLHelper
{
    public static List<View> ProcessHTML(string TheHTML)
    {
        //var stackLayout = new VerticalStackLayout();
        //stackLayout.Padding = new Thickness(0, 0, 0, 15);
        //Console.WriteLine($"");

        List<View> stackLayout = new List<View>();

        string pattern = @"<(?<tag>\w+)[^>]*>(?<content>.*?)<\/\k<tag>>";
        MatchCollection matches = Regex.Matches(TheHTML, pattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            string tagName = match.Groups["tag"].Value;
            string content = match.Groups["content"].Value;

            Console.WriteLine($"-- Update Content Match: {tagName}");

            Style? bodyTextSpanStyle = Application.Current?.Resources["BodyTextSpan"] as Style;

            if (tagName == "p")
            {
                Label label = new Label();
                label.FormattedText = new FormattedString();
                label = CreateSpans(content, label, bodyTextSpanStyle);
                stackLayout.Add(label);
            }
            else if (tagName == "ul")
            {
                VerticalStackLayout ul = processUL(content, bodyTextSpanStyle);
                stackLayout.Add(ul);
            }
            else
            {
                Console.WriteLine($"No path found for {tagName}: {content}");
            }
        }
        return stackLayout;
    }

    public static Label CreateSpans(string str, Label label, Style? bodyTextSpanStyle)
    {
        bool hasMatched = false;

        string linkPattern = @"^(.*?)<a\b[^>]*?href=['""](.*?)['""][^>]*>(.*?)<\/a>(.*)$";
        Match linkMatch = Regex.Match(str, linkPattern, RegexOptions.Singleline);

        if (linkMatch.Success && hasMatched == false)
        {
            hasMatched = true;
            string beforeLink = linkMatch.Groups[1].Value.Trim();
            string url = linkMatch.Groups[2].Value.Trim();
            string insideLink = linkMatch.Groups[3].Value.Trim();
            string afterLink = linkMatch.Groups[4].Value.Trim();

            Span span = new Span();
            if (!string.IsNullOrEmpty(beforeLink))
            {

                span = new Span
                {
                    Text = beforeLink,
                    Style = bodyTextSpanStyle
                };
                label.FormattedText.Spans.Add(span);
            }

            if (insideLink.Contains("<em>"))
            {
                int emIndex = insideLink.IndexOf("<em>");
                insideLink = insideLink.Remove(emIndex, 4);
            }
            if (insideLink.Contains("</em>"))
            {
                int emEndIndex = insideLink.IndexOf("</em>");
                insideLink = insideLink.Remove(emEndIndex, 5);
            }

            if (insideLink.Contains("<strong>"))
            {
                int strongIndex = insideLink.IndexOf("<strong>");
                insideLink = insideLink.Remove(strongIndex, 8);
            }
            if (insideLink.Contains("</strong>"))
            {
                int strongEndIndex = insideLink.IndexOf("</strong>");
                insideLink = insideLink.Remove(strongEndIndex, 9);
            }

            string text = insideLink;
            if (beforeLink.Length > 1)
            {
                span.Text = $"{span.Text} ";
            }
            // 

            Console.WriteLine($"Span Link: {insideLink}: {url}");
            Span linkSpan = new Span
            {
                Text = text,
                Style = Application.Current.Resources["BodyTextLinkSpan"] as Style
            };
            linkSpan.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await Browser.OpenAsync(new Uri(url), BrowserLaunchMode.SystemPreferred))
            });
            label.FormattedText.Spans.Add(linkSpan);

            if (afterLink.Length > 1)
            {
                label.FormattedText.Spans.Add(new Span
                {
                    Text = " ",
                    Style = bodyTextSpanStyle
                });
            }

            if (!string.IsNullOrEmpty(afterLink.Trim()))
            {
                return CreateSpans(afterLink.Trim(), label, bodyTextSpanStyle);
            }
        }

        string strongPattern = @"^(.*?)<(?:em|strong)>(.*?)</(?:em|strong)>(.*)$";
        Match strongMatch = Regex.Match(str.Trim(), strongPattern, RegexOptions.Singleline);

        if (strongMatch.Success && hasMatched == false && !str.ToLower().Contains("what3words"))
        {
            hasMatched = true;
            string beforeEm = strongMatch.Groups[1].Value.TrimStart();
            string insideEm = strongMatch.Groups[2].Value.Trim();
            string afterEm = strongMatch.Groups[3].Value.Trim();

            if (beforeEm.Length > 0)
            {
                Console.WriteLine($"Span: {beforeEm}");
                Span span = new Span
                {
                    Text = beforeEm,
                    Style = bodyTextSpanStyle
                };
                label.FormattedText.Spans.Add(span);
            }

            if (insideEm.StartsWith("<em>"))
                insideEm = insideEm.Substring(4);
            if (insideEm.StartsWith("<strong>"))
                insideEm = insideEm.Substring(8);

            string text = insideEm;
            if (afterEm.Length > 1)
            {
                text += " ";
            }

            Console.WriteLine($"Span Bold: {text}");
            Span span1 = new Span
            {
                Text = text,
                Style = Application.Current?.Resources["BodyTextBoldSpan"] as Style
            };
            label.FormattedText.Spans.Add(span1);

            // This isn't doing what I think. I think??
            if (afterEm.Trim().ToLower() == "</strong>")
                afterEm = afterEm.Substring(9).Trim();
            if (afterEm.Trim().ToLower() == "</em>")
                afterEm = afterEm.Substring(5).Trim();

            if (afterEm.Length > 0)
            {
                Console.WriteLine($"Span left over: {afterEm}");
                return CreateSpans(afterEm, label, bodyTextSpanStyle);
            }
        }
        if (!hasMatched)
        {
            Console.WriteLine($"Span: {str}");
            if (str.Contains("</br>"))
            {
                string[] parts = str.Split(new string[] { "</br>" }, StringSplitOptions.None);
                int counter = 0;
                foreach (string part in parts)
                {
                    Span span = new Span
                    {
                        Text = part,
                        Style = bodyTextSpanStyle
                    };
                    if (counter < parts.Length - 1)
                    {
                        span.Text += "\n";
                    }
                    counter++;
                    label.FormattedText.Spans.Add(span);
                }
            } else {
                Span span = new Span
                {
                    Text = str,
                    Style = bodyTextSpanStyle
                };
                label.FormattedText.Spans.Add(span);
            }
        }

        return label;
    }

    public static VerticalStackLayout processUL(string str, Style? bodyTextSpanStyle)
    {
        Console.WriteLine("");
        string pattern = @"<(?<tag>\w+)[^>]*>(?<content>.*?)<\/\k<tag>>";
        MatchCollection matches = Regex.Matches(str, pattern, RegexOptions.Singleline);

        VerticalStackLayout stackLayout = new VerticalStackLayout();

        foreach (Match match in matches)
        {
            string content = match.Groups["content"].Value;
            string tagName = match.Groups["tag"].Value;

            Label label = new Label();
            label.Style = Application.Current.Resources["BodyText"] as Style;
            if (tagName == "li")
            {
                if (content.Contains("<em>") || content.Contains("<strong>"))
                {
                    label = CreateSpans(content, label, bodyTextSpanStyle);
                }
                else
                {
                    label.Text = "- " + content;
                    Console.WriteLine($"LI: {content}");
                }
            }
            stackLayout.Children.Add(label);
        }
        return stackLayout;
    }
}
