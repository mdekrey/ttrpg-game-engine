import { EditableClassDescriptor } from 'src/api/models/EditableClassDescriptor';
import { useApi } from 'src/core/hooks/useApi';
import { ClassSurveyForm } from './form/class-survey-form';

export function ClassDetailsEditor() {
	const api = useApi();

	return (
		<div className="p-8 bg-gray-50 min-h-screen">
			<ClassSurveyForm onSubmit={submitClassDescriptor} />
		</div>
	);

	async function submitClassDescriptor(classProfile: EditableClassDescriptor) {
		const response = await api.createClass({ body: classProfile }).toPromise();
		if (response.statusCode === 200) {
			window.location.href = `/class/edit/${response.data.classProfileId}`;
		}
	}
}
