/* eslint-disable import/no-webpack-loader-syntax */
// NOTE: using loader syntax becuase Yaml worker imports editor.worker directly and that
// import shouldn't go through loader syntax.
import EditorWorker from 'worker-loader!monaco-editor/esm/vs/editor/editor.worker';
import YamlWorker from 'worker-loader!monaco-yaml/lib/esm/yaml.worker';
import { useEffect, useRef, useState } from 'react';
import { editor, Environment, IDisposable, Uri } from 'monaco-editor';
import { dump as toYaml, load as fromYaml } from 'js-yaml';
import { SchemasSettings, setDiagnosticsOptions } from 'monaco-yaml';
import api from 'api/api.yaml';

declare global {
	interface Window {
		MonacoEnvironment: Environment;
	}
}

window.MonacoEnvironment = {
	getWorker(moduleId, label) {
		switch (label) {
			case 'editorWorkerService':
				return new EditorWorker();
			case 'yaml':
				return new YamlWorker();
			default:
				throw new Error(`Unknown label ${label}`);
		}
	},
};

function toFullUrl(path: string) {
	const url = new URL(window.location.href);
	url.pathname = path;
	return url.href;
}

setDiagnosticsOptions({
	validate: true,
	enableSchemaRequest: true,
	hover: true,
	completion: true,
	schemas: [
		{
			uri: toFullUrl('/schemas/api.json'),
			fileMatch: [],
			schema: api as SchemasSettings['schema'],
		},
		{
			uri: toFullUrl('/schemas/tools.json'),
			fileMatch: ['tools.yaml'],
			schema: { $ref: `${toFullUrl('/schemas/api.json')}#/components/schemas/ClassProfile/properties/tools` },
		},
	],
});

export function YamlEditor<T>({ value, onChange, path }: { value: T; onChange?: (tools: T) => void; path?: string }) {
	const [isEditorReady, setIsEditorReady] = useState(false);
	const divRef = useRef<HTMLDivElement>(null);
	const subscriptionRef = useRef<IDisposable | null>(null);
	const valueRef = useRef<T>(value);
	const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null);

	useEffect(() => {
		if (!divRef.current) {
			editorRef.current?.dispose();
			editorRef.current = null;
			return;
		}
		const yamlValue = toYaml(value, {
			lineWidth: -1,
		});
		if (!editorRef.current) {
			editorRef.current = editor.create(divRef.current, {
				automaticLayout: true,
				model: editor.createModel(yamlValue, 'yaml', path === undefined ? undefined : Uri.parse(path)),
				theme: 'vs-light',
			});
			setIsEditorReady(true);
		} else if (JSON.stringify(valueRef.current) !== JSON.stringify(value)) {
			editorRef.current.setValue(yamlValue);
		}
	}, [value, path]);

	valueRef.current = value;

	useEffect(() => {
		const currentEditor = editorRef.current;
		if (isEditorReady && onChange && currentEditor) {
			subscriptionRef.current?.dispose();
			subscriptionRef.current =
				currentEditor.onDidChangeModelContent(() => {
					const editorValue = currentEditor.getValue();
					try {
						const result = fromYaml(editorValue) as T;

						if (JSON.stringify(valueRef.current) !== JSON.stringify(result)) {
							valueRef.current = result;
							onChange(result);
						}
					} catch (ex) {
						// eat the yaml error
					}
				}) || null;
		}
	}, [isEditorReady, onChange]);

	return <div style={{ height: '100%', width: '100%' }} ref={divRef} />;
}
