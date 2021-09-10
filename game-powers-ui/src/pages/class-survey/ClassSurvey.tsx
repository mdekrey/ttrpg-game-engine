import { ClassSurveyForm } from './form/class-survey-form';

export function ClassSurvey() {
	return (
		<div className="p-8 bg-gray-50">
			<ClassSurveyForm
				onSubmit={(classProfile) => {
					console.log(classProfile);
				}}
			/>
		</div>
	);
}
