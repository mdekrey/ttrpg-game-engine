import { createContext, useContext } from 'react';
import operations from 'api/operations';
import { toRxjsApi } from '@principlestudios/openapi-codegen-typescript-rxjs';
import { ajax } from 'rxjs/ajax';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

const ApiContext = createContext(
	toRxjsApi(operations, '', (request) => ajax(request).pipe(catchError((response) => of(response))))
);

export const ApiProvider = ApiContext.Provider;

export function useApi() {
	return useContext(ApiContext);
}
