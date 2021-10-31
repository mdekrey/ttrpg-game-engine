/* eslint-disable react/no-array-index-key */
import { useState } from 'react';
import { Button } from 'components/button/Button';
import { SelectField } from 'components/forms';
import { ButtonRow } from 'components/ButtonRow';
import { ClassProfile } from 'api/models/ClassProfile';
import { PowerFrequency } from 'api/models/PowerFrequency';
import { YamlEditor } from 'components/monaco/YamlEditor';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';
import { Modal } from 'components/modal/modal';
import { SamplePowerData, SamplePowers } from './SamplePowers';

export const powerLevelOptions: { level: number; usage: PowerFrequency }[] = [
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

export function SamplePowersSection({ classProfile }: { classProfile: ClassProfile }) {
	const [selectedCfg, setSelectedCfg] = useState<null | { toolIndex: number; powerConfigIndex: number }>(null);
	const [selectedLevel, setSelectedLevel] = useState<typeof powerLevelOptions[0]>(
		powerLevelOptions.find((p) => p.level === 19)!
	);
	const [selectedPower, setSelectedPower] = useState<null | SamplePowerData>(null);

	return (
		<>
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
				{classProfile.tools.map((tool, toolIndex) => (
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
			<Modal
				show={selectedPower != null}
				onClose={() => setSelectedPower(null)}
				title="Power Profile Helper"
				size="full">
				<div className="mt-2 grid grid-cols-3 gap-2">
					<div className="col-span-2">
						{selectedCfg && (
							<YamlEditor
								value={classProfile.tools[selectedCfg!.toolIndex].powerProfileConfigs[selectedCfg!.powerConfigIndex]}
								path="power-profile-config.yaml"
							/>
						)}
					</div>
					{selectedPower && (
						<PowerTextBlock
							{...selectedPower.power}
							powerUsage={selectedPower.power.powerUsage as PowerType}
							attackType={
								(selectedPower.power.attackType || null) as 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null
							}
						/>
					)}
				</div>

				<ButtonRow>
					<Button onClick={() => setSelectedPower(null)}>Done</Button>
				</ButtonRow>
			</Modal>
		</>
	);
}
