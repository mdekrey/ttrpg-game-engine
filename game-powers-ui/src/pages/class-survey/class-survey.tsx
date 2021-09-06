import { Label, Select, Textbox } from 'components/forms';
import { useId } from 'core/hooks/useId';
import { useState } from 'react';
import { Card } from '../../components/card/card';

const toolTypes = ['Weapon', 'Implement'];

export function ClassSurvey({ className }: { className?: string }) {
	const [selectedToolType, setSelectedToolType] = useState<string>(toolTypes[0]);
	const id = useId();

	return (
		<form className={className}>
			<Card>
				<div className="grid grid-cols-6 gap-6">
					<div className="col-span-6 sm:col-span-3">
						<Label htmlFor={`class-name-${id}`}>Name</Label>
						<Textbox id={`class-name-${id}`} />
					</div>
					<div className="col-span-6 sm:col-span-3">
						<Select
							value={selectedToolType}
							onChange={setSelectedToolType}
							options={toolTypes}
							optionKey={(opt) => opt}
							optionDisplay={(opt) => opt}
						/>
					</div>
				</div>
			</Card>
		</form>
	);
}
