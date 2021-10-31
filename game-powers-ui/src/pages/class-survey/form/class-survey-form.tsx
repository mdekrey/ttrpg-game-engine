/* eslint-disable react/no-array-index-key */
import { Fragment, useMemo, useState } from 'react';
import { useGameForm } from 'core/hooks/useGameForm';
import { Button } from 'components/button/Button';
import { Card } from 'components/card/card';
import { SelectField, SelectFormField, TextboxField } from 'components/forms';
import { ButtonRow } from 'components/ButtonRow';
import { ClassProfile } from 'api/models/ClassProfile';
import { PowerFrequency } from 'api/models/PowerFrequency';
import { ToolProfile } from 'api/models/ToolProfile';
import { classSurveySchemaWithoutTools, roles } from 'core/schemas/api';
import { YamlEditor } from 'components/monaco/YamlEditor';
import { Dialog, Transition } from '@headlessui/react';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';
import { SamplePowerData, SamplePowers } from './SamplePowers';

const defaultToolProfile: Readonly<ToolProfile> = {
	toolType: 'Weapon',
	toolRange: 'Melee',
	abilities: ['Strength'],
	preferredDamageTypes: ['Normal'],
	powerProfileConfigs: [
		{
			name: 'Any Power',
			powerChances: [{ selector: '$', weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Accurate',
			powerChances: [
				{ selector: "$..[?(@.Name=='Non-Armor Defense' || @.Name=='To-Hit Bonus to Current Attack')]", weight: 1.0 },
			],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Follow-up Attack',
			powerChances: [
				{ selector: "$..[?(@.Name=='RequiredHitForNextAttack' || @.Name=='RequiresPreviousHit')]", weight: 1.0 },
			],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Two Attacks',
			powerChances: [{ selector: "$..[?(@.Name=='TwoHits')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Multiple Attacks',
			powerChances: [{ selector: "$..[?(@.Name=='UpToThreeTargets')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Multiattack (does this get triggered?)',
			powerChances: [{ selector: "$..[?(@.Name=='Multiattack')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Apply conditions',
			powerChances: [{ selector: "$..[?(@.Name=='Condition')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Reactions',
			powerChances: [{ selector: "$..[?(@.Name=='OpportunityAction')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
		{
			name: 'Maneuver',
			powerChances: [{ selector: "$..[?(@.Name=='Skirmish Movement')]", weight: 1.0 }],
			modifierChances: [{ selector: '$', weight: 1.0 }],
		},
	],
};

const powerLevelOptions: { level: number; usage: PowerFrequency }[] = [
	{ level: 1, usage: 'At-Will' },
	{ level: 1, usage: 'Encounter' },
	{ level: 1, usage: 'Daily' },
	{ level: 3, usage: 'Encounter' },
	{ level: 5, usage: 'Daily' },
	{ level: 7, usage: 'Encounter' },
	{ level: 9, usage: 'Daily' },
	{ level: 11, usage: 'Encounter' },
	{ level: 13, usage: 'Encounter' },
	{ level: 15, usage: 'Daily' },
	{ level: 17, usage: 'Encounter' },
	{ level: 19, usage: 'Daily' },
	{ level: 20, usage: 'Daily' },
	{ level: 23, usage: 'Encounter' },
	{ level: 25, usage: 'Daily' },
	{ level: 27, usage: 'Encounter' },
	{ level: 29, usage: 'Daily' },
];

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
	const [selectedCfg, setSelectedCfg] = useState<null | { toolIndex: number; powerConfigIndex: number }>(null);
	const [selectedLevel, setSelectedLevel] = useState<typeof powerLevelOptions[0]>(
		powerLevelOptions.find((p) => p.level === 19)!
	);
	const [selectedPower, setSelectedPower] = useState<null | SamplePowerData>(null);
	const { handleSubmit, ...form } = useGameForm<Omit<ClassProfile, 'tools'>>({
		defaultValues: defaultValues || {
			name: 'Custom Class',
			role: 'Controller',
			powerSource: 'Martial',
		},
		schema: classSurveySchemaWithoutTools,
	});

	const role = form.watch('role');
	const powerSource = form.watch('powerSource');
	const classProfile: ClassProfile = useMemo(
		() => ({ name: 'Unimportant', role, powerSource, tools }),
		[role, powerSource, tools]
	);

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
				<SelectFormField className="col-span-6 sm:col-span-3" label="Role" form={form} name="role">
					{roles.map((r) => (
						<option key={r} value={r}>
							{r}
						</option>
					))}
				</SelectFormField>
				<TextboxField label="PowerSource" className="col-span-6 sm:col-span-3" form={form} name="powerSource" />
				<div className="col-span-6 h-96">
					<YamlEditor value={tools} onChange={setTools} path="tools.yaml" />
				</div>
			</Card>
			<Card className="grid grid-cols-6 gap-6 mt-6">
				<SelectField
					className="col-span-3"
					label="Preview Powers"
					value={selectedCfg === null ? '-' : `${selectedCfg.toolIndex}-${selectedCfg.powerConfigIndex}`}
					onChange={({ currentTarget: { value } }) => {
						if (value) {
							const [toolIndex, powerConfigIndex] = value.split('-').map(Number);
							setSelectedCfg({ toolIndex, powerConfigIndex });
						} else setSelectedCfg(null);
					}}>
					<option value="">None</option>
					{tools.map((tool, toolIndex) => (
						<optgroup label={`${tool.toolRange} ${tool.toolType}`} key={toolIndex}>
							{tool.powerProfileConfigs.map((powerConfig, powerConfigIndex) => (
								<option key={powerConfigIndex} value={`${toolIndex}-${powerConfigIndex}`}>
									{powerConfig.name}
								</option>
							))}
						</optgroup>
					))}
				</SelectField>
				<SelectField
					className="col-span-3"
					label="Power Level"
					value={`${selectedLevel.level}-${selectedLevel.usage}`}
					onChange={({ currentTarget: { value } }) => {
						const [level, usage] = value.split('-');
						setSelectedLevel({ level: Number(level), usage: usage as PowerFrequency });
					}}>
					{powerLevelOptions.map((cfg) => (
						<option key={`${cfg.level}-${cfg.usage}`} value={`${cfg.level}-${cfg.usage}`}>
							{`Lvl ${cfg.level} ${cfg.usage}`}
						</option>
					))}
				</SelectField>
				<div className="col-span-6">
					{selectedCfg && selectedLevel && (
						<SamplePowers
							classProfile={classProfile}
							toolIndex={selectedCfg.toolIndex}
							powerProfileIndex={selectedCfg.powerConfigIndex}
							level={selectedLevel.level}
							usage={selectedLevel.usage}
							onSelectPower={(p) => setSelectedPower(p)}
						/>
					)}
				</div>
			</Card>
			<Card className="grid grid-cols-6 gap-6 mt-6">
				<ButtonRow className="col-span-6">
					<Button type="submit">Submit</Button>
				</ButtonRow>
			</Card>

			<Transition appear show={selectedPower != null} as={Fragment}>
				<Dialog as="div" className="fixed inset-0 z-10 overflow-y-auto" onClose={() => setSelectedPower(null)}>
					<div className="min-h-screen px-4 text-center">
						<Transition.Child
							as={Fragment}
							enter="ease-out duration-300"
							enterFrom="opacity-0"
							enterTo="opacity-100"
							leave="ease-in duration-200"
							leaveFrom="opacity-100"
							leaveTo="opacity-0">
							<Dialog.Overlay className="fixed inset-0 bg-black bg-opacity-70" />
						</Transition.Child>

						{/* This element is to trick the browser into centering the modal contents. */}
						<span className="inline-block h-screen align-middle" aria-hidden="true">
							&#8203;
						</span>
						<Transition.Child
							as={Fragment}
							enter="ease-out duration-300"
							enterFrom="opacity-0 scale-95"
							enterTo="opacity-100 scale-100"
							leave="ease-in duration-200"
							leaveFrom="opacity-100 scale-100"
							leaveTo="opacity-0 scale-95">
							<div className="inline-block w-full max-w-screen-xl p-6 my-8 overflow-hidden text-left align-middle transition-all transform bg-white shadow-xl rounded-md">
								<Dialog.Title as="h3" className="text-lg font-medium leading-6 text-gray-900">
									Power Profile Helper
								</Dialog.Title>
								<div className="mt-2 grid grid-cols-3 gap-2">
									{selectedCfg && (
										<div className="col-span-2">
											<YamlEditor
												value={tools[selectedCfg!.toolIndex].powerProfileConfigs[selectedCfg!.powerConfigIndex]}
												path="power-profile-config.yaml"
											/>
										</div>
									)}
									{selectedPower && (
										<PowerTextBlock
											{...selectedPower.power}
											powerUsage={selectedPower.power.powerUsage as PowerType}
											attackType={
												(selectedPower.power.attackType || null) as
													| 'Personal'
													| 'Ranged'
													| 'Melee'
													| 'Close'
													| 'Area'
													| null
											}
										/>
									)}
								</div>

								<ButtonRow>
									<Button onClick={() => setSelectedPower(null)}>Done</Button>
								</ButtonRow>
							</div>
						</Transition.Child>
					</div>
				</Dialog>
			</Transition>
		</form>
	);
}
