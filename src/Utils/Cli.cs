/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Utils;

public class Cli
{
	public readonly struct StringResponse
	{
		private readonly string? value;

		[Pure]
		private StringResponse(string? value)
		{
			this.value = value;
		}

		[Pure] public static StringResponse Eof() => new StringResponse(null);
		[Pure] public static StringResponse OfValue(string value) => new StringResponse(value);

		[Pure]
		public bool IsEof()
		{
			return (this.value is null);
		}

		[Pure]
		public string GetValue()
		{
			if (this.value is null)
			{
				throw new InvalidOperationException("Cannot retrieve the response's value; response was EOF");
			}

			return this.value;
		}
	}

	public readonly struct BoolResponse
	{
		private readonly bool? value;

		[Pure]
		private BoolResponse(bool? value)
		{
			this.value = value;
		}

		[Pure] public static BoolResponse Eof() => new BoolResponse(null);
		[Pure] public static BoolResponse OfValue(bool value) => new BoolResponse(value);

		[Pure]
		public bool IsEof()
		{
			return (this.value is null);
		}

		[Pure]
		public bool IsYes()
		{
			if (this.value is null)
			{
				throw new InvalidOperationException("Cannot retrieve the response's value; response was EOF");
			}

			return this.value.Value;
		}
	}

	public class Paragraph : IDisposable
	{
		public class Action
		{
			private readonly Paragraph paragraphRef;
			private bool finished = false;

			[Pure]
			private Action(Paragraph paragraphRef)
			{
				this.paragraphRef = paragraphRef;
			}

			public static Action StartNew(Paragraph paragraphRef, string name)
			{
				if (paragraphRef.currentAction is not null)
				{
					string msg = "Must not start a new action while the current one is not finished";
					throw new InvalidOperationException(msg);
				}

				paragraphRef.printLineInternal(msg: $"{name}...", newLine: false);

				return (paragraphRef.currentAction = new Action(paragraphRef));
			}

			public void Finish(string resultMsg)
			{
				if (this.finished)
				{
					throw new InvalidOperationException("Action is already finished");
				}
				this.finished = true;

				this.paragraphRef.currentAction = null;
				this.paragraphRef.printLineInternal(" " + resultMsg, newLine: true);
			}
		}

		private readonly Cli cliRef;
		private Action? currentAction = null;

		[Pure]
		private Paragraph(Cli cliRef)
		{
			this.cliRef = cliRef;
		}

		public static Paragraph BeginNew(Cli cliRef)
		{
			if (cliRef.currentParagraph is not null)
			{
				string msg = "Must not start a new paragraph while the current one has not ended";
				throw new InvalidOperationException(msg);
			}

			cliRef.blankLineRequested = true;

			return (cliRef.currentParagraph = new Paragraph(cliRef));
		}

		public void PrintLine(string msg)
		{
			this.ensureNoActiveAction(forbiddenAction: "print a line");
			this.printLineInternal(msg, newLine: true);
		}

		public void PrintBlankLine()
		{
			this.ensureNoActiveAction(forbiddenAction: "print a blank line");
			this.cliRef.printBlankLineInternal();
		}

		public Action StartNewAction(string name)
		{
			return Action.StartNew(paragraphRef: this, name);
		}

		#region prompting

		public StringResponse PromptForString(string msg)
		{
			this.ensureNoActiveAction(forbiddenAction: "prompt for string");

			string? rawResponse = prompt(msg + ": ");

			if (rawResponse is null)
			{
				return StringResponse.Eof();
			}

			return StringResponse.OfValue(rawResponse.Trim());
		}

		public StringResponse PromptForString(string msg, string defaultValue)
		{
			this.ensureNoActiveAction(forbiddenAction: "prompt for string");

			if (defaultValue == string.Empty)
			{
				throw new ArgumentException(
					message: "Default value must not be empty",
					paramName: nameof(defaultValue)
				);
			}

			string? rawResponse = prompt($"{msg}: [{defaultValue}] ");

			if (rawResponse is null)
			{
				return StringResponse.Eof();
			}

			string responseValue = rawResponse.Trim();

			if (responseValue == string.Empty)
			{
				responseValue = defaultValue;
			}

			return StringResponse.OfValue(responseValue);
		}

		public BoolResponse PromptForBool(string msg, bool defaultValue)
		{
			this.ensureNoActiveAction(forbiddenAction: "prompt for bool");

			string? rawResponse = this.prompt(msg + (defaultValue ? " [Y/n] " : " [y/N] "));

			if (rawResponse is null)
			{
				return BoolResponse.Eof();
			}

			string trimmedResponse = rawResponse.Trim();

			bool responseValue;
			if (trimmedResponse != string.Empty)
			{
				responseValue = trimmedResponse.ToLower().StartsWith("y");
			}
			else
			{
				responseValue = defaultValue;
			}

			return BoolResponse.OfValue(responseValue);
		}

		private string? prompt(string msg)
		{
			this.printLineInternal(msg, newLine: false);
			return Console.ReadLine();
		}

		#endregion

		private void ensureNoActiveAction(string forbiddenAction)
		{
			if (this.currentAction is null)
			{
				return;
			}

			string msg = $"Must not {forbiddenAction} while the current action is not finished";
			throw new InvalidOperationException(msg);
		}

		private void printLineInternal(string msg, bool newLine)
		{
			this.cliRef.printLineInternal(msg, newLine);
		}

		void IDisposable.Dispose()
		{
			this.cliRef.currentParagraph = null;
		}
	}

	private bool blankLineWasPrinted = true;
	private bool blankLineRequested = false;
	private Paragraph? currentParagraph = null;

	public Paragraph BeginNewParagraph()
	{
		return Paragraph.BeginNew(cliRef: this);
	}

	public void PrintBlankLine()
	{
		if (this.currentParagraph is not null)
		{
			string msg = "Must not print a blank line through the Cli object while the current paragraph has not ended";
			throw new InvalidOperationException(msg);
		}

		this.printBlankLineInternal();
	}

	private void printBlankLineInternal()
	{
		Console.Error.WriteLine();
		this.blankLineWasPrinted = true;
	}

	private void printLineInternal(string msg, bool newLine)
	{
		if (msg == string.Empty)
		{
			throw new ArgumentException(
				message: "Message must not be empty",
				paramName: nameof(msg)
			);
		}

		if (this.blankLineRequested)
		{
			if (!(this.blankLineWasPrinted))
			{
				this.printBlankLineInternal();
			}

			this.blankLineRequested = false;
		}

		if (newLine)
		{
			Console.Error.WriteLine(msg);
		}
		else
		{
			Console.Error.Write(msg);
		}
		this.blankLineWasPrinted = false;
	}
}
