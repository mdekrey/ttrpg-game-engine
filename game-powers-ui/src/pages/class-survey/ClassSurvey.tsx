import { EditableClassProfile } from 'api/models/EditableClassProfile';
import { useApi } from 'core/hooks/useApi';
import { ClassSurveyForm } from './form/class-survey-form';

export function ClassSurvey() {
	const api = useApi();

	return (
		<div className="p-8 bg-gray-50 min-h-screen">
			<ClassSurveyForm onSubmit={submitClassProfile} />
		</div>
	);

	async function submitClassProfile(classProfile: EditableClassProfile) {
		const response = await api.generatePowers({ body: classProfile }).toPromise();
		if (response.statusCode === 200) {
			window.location.href = `/class/edit/${response.data.classProfileId}`;
		}
	}
}
