﻿namespace SEOLib

open System.Text.RegularExpressions
open Utilities

module Html =

    /// <summary>Returns the title of a HTML document.</summary>
    /// <param name="html">The HTML document.</param>
    /// <returns>The content of the title element.</returns>
    let title html =
        sndGroupValue titleRegex html
        |> checkEmptyString'

    /// <summary>Returns the meta elements of a HTML document as a string array.</summary>
    /// <param name="html">The HTML document.</param>
    /// <returns>The meta elements.</returns>
    let metaTags html =
        metaTagRegex.Matches html
        |> Seq.cast<Match>
        |> Seq.toArray
        |> Array.map fstGroupValue'

    /// <summary>Returns the content of the meta description element.</summary>
    /// <param name="metaTags">The meta elements as a string array.</param>
    /// <returns>The meta description content.</returns>
    let metaDescription metaTags =
        metaTag metaTags metaDescRegex contentAttrRegex

    /// <summary>Returns the content of the meta keywords element.</summary>
    /// <param name="metaTags">The meta elements as a string array.</param>
    /// <returns>The meta keywords content.</returns>
    let metaKeywords metaTags =
        metaTag metaTags metaKeysRegex contentAttrRegex

    let private stripHtml html =
        commentJSCssRegex.Replace(html, "")
        |> stripTags

    /// <summary>Returns the headings of a HTML document.</summary>
    /// <param name="html">The HTML document.</param>
    /// <returns>The headings of the document.</returns>    
    let headings html =
        headingRegex.Matches html
        |> Seq.cast<Match>
        |> Seq.toList
        |> List.map (fun x ->
            let sndGroup = sndGroupValue' x
            let trdGroup = trdGroupValue' x |> stripHtml |> decodeHtml
            sndGroup, trdGroup)
        |> List.filter (fun (_, x) -> x.Length > 0)
        |> List.map (fun (x, y) -> makeHeading x y)
        
    let private stripSpaces html =
        let regex = compileRegex "(\n|\r)"
        let regex' = compileRegex " {2,}"
        regex.Replace(html, " ")
        |> fun x -> regex'.Replace(x, " ")
    
    let private f = stripHtml >> stripSpaces >> decodeHtml

    /// <summary>Calculates the text/HTML ratio in a HTML document.</summary>
    /// <param name="html">The HTML document.</param>
    /// <returns>The text/HTML ratio.</returns>
    let textHtmlRatio html =
        let textLength =
            let text = f html //stripHtml html |> stripSpaces |> decodeHtml
            float text.Length
        let htmlLength = float html.Length
        textLength / htmlLength * 100. |> round