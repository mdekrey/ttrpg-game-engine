/* eslint-disable import/no-webpack-loader-syntax */
// NOTE: using loader syntax becuase Yaml worker imports editor.worker directly and that
// import shouldn't go through loader syntax.
import EditorWorker from 'worker-loader!monaco-editor/esm/vs/editor/editor.worker';
import YamlWorker from 'worker-loader!monaco-yaml/lib/esm/yaml.worker';
import { useEffect, useRef, useState } from 'react';
import { editor, Environment, IDisposable } from 'monaco-editor';
import { dump as toYaml, load as fromYaml } from 'js-yaml';
import { SchemasSettings, setDiagnosticsOptions } from 'monaco-yaml';
import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { SelectField, TextboxField } from 'components/forms';
import { ButtonRow } from 'components/ButtonRow';
import { ClassProfile } from 'api/models/ClassProfile';
import { ToolProfile } from 'api/models/ToolProfile';
import { classSurveySchemaWithoutTools, roles } from 'core/schemas/api';

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

setDiagnosticsOptions({
	validate: true,
	enableSchemaRequest: true,
	hover: true,
	completion: true,
	schemas: [],
});

function YamlEditor<T>({ value, onChange }: { value: T; onChange?: (tools: T) => void }) {
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
		const yamlValue = toYaml(value);
		if (!editorRef.current) {
			editorRef.current = editor.create(divRef.current, {
				automaticLayout: true,
				model: editor.createModel(yamlValue, 'yaml'),
				theme: 'vs-light',
			});
			setIsEditorReady(true);
		} else if (JSON.stringify(valueRef.current) !== JSON.stringify(value)) {
			editorRef.current.setValue(yamlValue);
		}
	}, [value]);

	valueRef.current = value;

	useEffect(() => {
		const currentEditor = editorRef.current;
		if (isEditorReady && onChange && currentEditor) {
			subscriptionRef.current?.dispose();
			subscriptionRef.current =
				currentEditor.onDidChangeModelContent(() => {
					const editorValue = currentEditor.getValue();
					const result = fromYaml(editorValue) as T;

					if (JSON.stringify(valueRef.current) !== JSON.stringify(result)) {
						valueRef.current = result;
						onChange(result);
					}
				}) || null;
		}
	}, [isEditorReady, onChange]);

	return <div style={{ height: '100%', width: '100%' }} ref={divRef} />;
}

const defaultToolProfile: Readonly<ToolProfile> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: ['Strength'],
	preferredDamageTypes: ['Normal'],
	powerProfileConfig: {
		powerChances: [
			{ selector: "$..[?(@.Name=='Non-Armor Defense' || @.Name=='To-Hit Bonus to Current Attack')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='RequiredHitForNextAttack')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='RequiresPreviousHit')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='TwoHits')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='UpToThreeTargets')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='Multiattack')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='Condition')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='OpportunityAction')]", weight: 1.0 },
			{ selector: "$..[?(@.Name=='Skirmish Movement')]", weight: 1.0 },
		],
		modifierChances: [{ selector: '$', weight: 1.0 }],
	},
};

export function ClassSurveyForm({
	className,
	onSubmit,
	defaultValues,
}: {
	className?: string;
	onSubmit?: (form: ClassProfile) => void;
	defaultValues?: ClassProfile;
}) {
	const [tools, setTools] = useState([defaultToolProfile]);
	const { handleSubmit, ...form } = useGameForm<Omit<ClassProfile, 'tools'>>({
		defaultValues: defaultValues || {
			name: 'Custom Class',
			role: 'Controller',
			powerSource: 'Martial',
		},
		schema: classSurveySchemaWithoutTools,
	});

	return (
		<form
			className={className}
			onSubmit={
				onSubmit &&
				handleSubmit((value) => {
					onSubmit({ ...value, tools });
				})
			}>
			<Card className="grid grid-cols-6 gap-6">
				<TextboxField label="Class Name" className="col-span-6 sm:col-span-3" form={form} name="name" />
				<SelectField className="col-span-6 sm:col-span-3" label="Role" form={form} name="role" options={roles} />
				<TextboxField label="PowerSource" className="col-span-6 sm:col-span-3" form={form} name="powerSource" />
				<div className="col-span-6 h-screen">
					<YamlEditor value={tools} onChange={setTools} />
				</div>
				<ButtonRow className="col-span-6">
					<Button type="submit">Submit</Button>
				</ButtonRow>
			</Card>
		</form>
	);
}
