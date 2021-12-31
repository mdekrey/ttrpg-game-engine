import { editor, Uri } from 'monaco-editor';

export function getModelWithContent(path: string | undefined, language: string | undefined, content: string) {
	const uri = path === undefined ? undefined : Uri.parse(path);
	const existing = uri ? editor.getModel(uri) : null;
	if (existing) {
		existing.setValue(content);
		return existing;
	}
	return editor.createModel(content, language, path === undefined ? undefined : Uri.parse(path));
}
