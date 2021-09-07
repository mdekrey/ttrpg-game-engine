import { ComponentType, ExoticComponent } from 'react';
import { Controller, ControllerRenderProps, FieldValues, Path, UseControllerProps } from 'react-hook-form';

export function Controlled<P>(
	Component: ExoticComponent<P> | ComponentType<P>
): <TFieldValues extends FieldValues = FieldValues, TName extends Path<TFieldValues> = Path<TFieldValues>>(
	props: Omit<P, keyof ControllerRenderProps> & UseControllerProps<TFieldValues, TName>
) => JSX.Element {
	return ({ name, rules, shouldUnregister, defaultValue, control, ...props }) => (
		<Controller
			name={name}
			rules={rules}
			shouldUnregister={shouldUnregister}
			defaultValue={defaultValue}
			control={control}
			render={({ field }) => <Component {...props} {...(field as any)} />}
		/>
	);
}
