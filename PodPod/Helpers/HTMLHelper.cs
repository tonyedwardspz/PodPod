using System.Text.RegularExpressions;
using ObjCRuntime;

namespace PodPod.Services;

public static class HTMLHelper
{
    public static VerticalStackLayout ProcessHTML(string TheHTML)
    {
        VerticalStackLayout stackLayout = new VerticalStackLayout();
        Style? bodyTextSpanStyle = Application.Current?.Resources["BodyTextSpan"] as Style;

        string pattern = @"<(?<tag>\w+)[^>]*>(?<content>.*?)<\/\k<tag>>";
        MatchCollection matches = Regex.Matches(TheHTML, pattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            string tagName = match.Groups["tag"].Value;
            string content = match.Groups["content"].Value;
            Console.WriteLine($"-- Update Content Match: {tagName}");

            if (tagName == "p")
            {
                Label label = new Label();
                label.FormattedText = new FormattedString();
                label = CreateSpans(content, label, bodyTextSpanStyle);
                stackLayout.Children.Add(label);
            }
            else if (tagName == "ul")
            {
                VerticalStackLayout ul = processUL(content, bodyTextSpanStyle);
                stackLayout.Children.Add(ul);
            }
            else
            {
                Console.WriteLine($"No path found for {tagName}: {content}");
            }
        }
        if (matches.Count == 0)
        {
            Label label = new();
            label.FormattedText = new FormattedString();
            label = CreateSpans(TheHTML, label, bodyTextSpanStyle);
            stackLayout.Children.Add(label);
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

            LinkObject linkObject = new LinkObject
            {
                BeforeLink = beforeLink,
                Url = url,
                InsideLink = insideLink,
                AfterLink = afterLink
            };
            return ProcessLink(label, bodyTextSpanStyle, linkObject);
        }

        string pattern = @"(?<!<a\s[^>]*?>)(?<before>.*?)(?<url>https?:\/\/\S+?)(?<after>(\s.*?|$))(?![^<]*<\/a>)";


        Match NonAnchorLink = Regex.Match(str, pattern);

        if (NonAnchorLink.Success && hasMatched == false)
        {
            string before = NonAnchorLink.Groups["before"].Value.Trim();
            string url = NonAnchorLink.Groups["url"].Value.Trim();
            string after = NonAnchorLink.Groups["after"].Value.Trim();

            hasMatched = true;

            LinkObject linkObject = new LinkObject
            {
                BeforeLink = before,
                Url = url,
                InsideLink = url,
                AfterLink = after
            };
            return ProcessLink(label, bodyTextSpanStyle, linkObject);
        } 
        
        if (!hasMatched)
        {
            //Console.WriteLine($"Span: {str}");
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

    private static Label ProcessLink(Label label, Style? bodyTextSpanStyle, LinkObject link)
    {
        Span span = new Span();
        if (!string.IsNullOrEmpty(link.BeforeLink))
        {
            span = new Span
            {
                Text = link.BeforeLink,
                Style = bodyTextSpanStyle
            };
            label.FormattedText.Spans.Add(span);
        }

        if (!string.IsNullOrEmpty(link.InsideLink))
        {

            if (link.InsideLink.Contains("<em>"))
            {
                int emIndex = link.InsideLink.IndexOf("<em>");
                link.InsideLink = link.InsideLink.Remove(emIndex, 4);
            }
            if (link.InsideLink.Contains("</em>"))
            {
                int emEndIndex = link.InsideLink.IndexOf("</em>");
                link.InsideLink = link.InsideLink.Remove(emEndIndex, 5);
            }

            if (link.InsideLink.Contains("<strong>"))
            {
                int strongIndex = link.InsideLink.IndexOf("<strong>");
                link.InsideLink = link.InsideLink.Remove(strongIndex, 8);
            }
            if (link.InsideLink.Contains("</strong>"))
            {
                int strongEndIndex = link.InsideLink.IndexOf("</strong>");
                link.InsideLink = link.InsideLink.Remove(strongEndIndex, 9);
            }
        } else {
            link.InsideLink = link.Url;
        }

        // add a space before the link if there is text before it
        if (link.BeforeLink.Length > 1)
        {
            span.Text = $"{span.Text} ";
        }

        Span linkSpan = new Span
        {
            Text = link.InsideLink,
            Style = Application.Current.Resources["BodyTextLinkSpan"] as Style
        };
        linkSpan.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Browser.OpenAsync(new Uri(link.Url)))
        });
        label.FormattedText.Spans.Add(linkSpan);

        if (link.AfterLink.Length > 1)
        {
            label.FormattedText.Spans.Add(new Span
            {
                Text = " ",
                Style = bodyTextSpanStyle
            });
            return CreateSpans(link.AfterLink, label, bodyTextSpanStyle);
        } else {
            label.FormattedText.Spans.Last().Text += link.AfterLink;
        }
        return label;
    }

    public static VerticalStackLayout processUL(string str, Style? bodyTextSpanStyle)
    {
        //Console.WriteLine("");
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
                    //Console.WriteLine($"LI: {content}");
                }
            }
            stackLayout.Children.Add(label);
        }
        return stackLayout;
    }
}

 public class LinkObject
    {
        public string BeforeLink { get; set; }
        public string Url { get; set; }
        public string InsideLink { get; set; }
        public string AfterLink { get; set; }
    }