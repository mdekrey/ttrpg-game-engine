/* eslint-disable import/no-webpack-loader-syntax */
// NOTE: using loader syntax becuase Yaml worker imports editor.worker directly and that
// import shouldn't go through loader syntax.
import { useEffect, useRef, useState } from 'react';
import { editor, IDisposable } from 'monaco-editor';
import { getModelWithContent } from './getModelWithContent';

export function MdxEditor({ value, onChange }: { value: string; onChange?: (tools: string) => void }) {
	const [isEditorReady, setIsEditorReady] = useState(false);
	const divRef = useRef<HTMLDivElement>(null);
	const subscriptionRef = useRef<IDisposable | null>(null);
	const valueRef = useRef<string>(value);
	const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null);

	useEffect(() => {
		if (!divRef.current) {
			editorRef.current?.dispose();
			editorRef.current = null;
			return;
		}
		if (!editorRef.current) {
			editorRef.current = editor.create(divRef.current, {
				automaticLayout: true,
				model: getModelWithContent(undefined, 'mdx', value),
				theme: 'vs-light',
			});
			setIsEditorReady(true);
		} else if (JSON.stringify(valueRef.current) !== JSON.stringify(value)) {
			editorRef.current.setValue(value);
		}
		valueRef.current = value;
	}, [value]);

	useEffect(() => {
		const currentEditor = editorRef.current;
		if (isEditorReady && onChange && currentEditor) {
			subscriptionRef.current?.dispose();
			subscriptionRef.current =
				currentEditor.onDidChangeModelContent(() => {
					const editorValue = currentEditor.getValue();

					valueRef.current = editorValue;
					onChange(editorValue);
				}) || null;
		}
	}, [isEditorReady, onChange]);

	return <div style={{ height: '100%', width: '100%' }} ref={divRef} />;
}
