import { Injectable } from '@angular/core'
import { Observable, BehaviorSubject, of } from 'rxjs'
import { map, catchError, tap, switchMap } from 'rxjs/operators'
import { ApiService } from './api.service'
import { environment } from '../../../environments/environment'
import { AIModel, AIModelSearchParams, AIModelRequest } from '../entities/ai-model'
import { AIModelResponse, AIModelsResponse } from '../entities/responses/ai-model-response'
import { AddUpdateEntityResponse } from '../entities/common/add-update-entity-response'
import { DeleteEntityResponse } from '../entities/common/delete-entity-response'

/**
 * Service for managing AI Models with comprehensive API integration
 * Based on the Swagger API specification for AIModel endpoints
 */
@Injectable({
    providedIn: 'root'
})
export class AIModelService {
    private baseUrl = environment.chatApiBaseUrl;
    private currentModelSubject = new BehaviorSubject<AIModel | null>(null);
    private modelsSubject = new BehaviorSubject<AIModel[]>([]);
    private loadingSubject = new BehaviorSubject<boolean>(false);

    public currentModel$ = this.currentModelSubject.asObservable();
    public models$ = this.modelsSubject.asObservable();
    public loading$ = this.loadingSubject.asObservable();

    constructor(private apiService: ApiService) {
    }

    // ========================
    // API Methods
    // ========================

    /**
     * GET /api/AIModel
     * Get all AI models
     */
    getAllModels(): Observable<AIModel[]> {
        this.loadingSubject.next(true)

        return this.apiService.get<AIModel[]>(this.baseUrl, 'AIModel').pipe(
            map(models => {
                this.loadingSubject.next(false)
                // The interceptor automatically unwraps successful responses
                const modelArray = Array.isArray(models) ? models : []
                this.modelsSubject.next(modelArray)
                return modelArray
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error fetching all AI models:', error)
                this.modelsSubject.next([])
                return of([])
            })
        )
    }

    /**
     * GET /api/AIModel/{id}
     * Get a specific AI model by ID
     */
    getModelById(id: number): Observable<AIModel> {
        this.loadingSubject.next(true)

        return this.apiService.get<AIModel>(this.baseUrl, `AIModel/${id}`).pipe(
            map(model => {
                this.loadingSubject.next(false)
                // The interceptor automatically unwraps successful responses
                if (model) {
                    this.currentModelSubject.next(model)
                    return model
                }
                throw new Error('AI Model not found')
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error fetching AI model by ID:', error)
                throw error
            })
        )
    }

    /**
     * POST /api/AIModel
     * Create a new AI model
     */
    createModel(model: AIModelRequest): Observable<number> {
        this.loadingSubject.next(true)

        return this.apiService.post<number>(this.baseUrl, 'AIModel', model).pipe(
            map(id => {
                this.loadingSubject.next(false)
                if (id && id > 0) {
                    // Refresh models list (fire and forget)
                    this.getAllModels().subscribe()
                }
                return id
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error creating AI model:', error)
                throw error
            })
        )
    }

    /**
     * PUT /api/AIModel/{id}
     * Update an existing AI model
     */
    updateModel(id: number, model: AIModelRequest): Observable<number> {
        this.loadingSubject.next(true)

        const updatePayload = { ...model, id }

        return this.apiService.put<number>(this.baseUrl, `AIModel/${id}`, updatePayload).pipe(
            map(updatedId => {
                this.loadingSubject.next(false)
                if (updatedId && updatedId > 0) {
                    // Refresh current model if it's the one being updated
                    if (this.currentModelSubject.value?.id === id) {
                        this.getModelById(id).subscribe()
                    }
                    // Refresh models list
                    this.getAllModels().subscribe()
                    return updatedId
                }
                throw new Error('Failed to update AI model')
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error updating AI model:', error)
                throw error
            })
        )
    }

    /**
     * DELETE /api/AIModel/{id}
     * Delete an AI model
     */
    deleteModel(id: number, modifiedBy: number = 0): Observable<string> {
        this.loadingSubject.next(true)

        const url = `AIModel/${id}?modifiedBy=${modifiedBy}`
        return this.apiService.delete<string>(this.baseUrl, url).pipe(
            map(response => {
                this.loadingSubject.next(false)
                if (response) {
                    // Clear current model if it's the one being deleted
                    if (this.currentModelSubject.value?.id === id) {
                        this.currentModelSubject.next(null)
                    }
                    // Refresh models list
                    this.getAllModels().subscribe()
                    return response || 'AI Model deleted successfully'
                }
                throw new Error('Failed to delete AI model')
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error deleting AI model:', error)
                throw error
            })
        )
    }

    /**
     * GET /api/AIModel/active
     * Get all active AI models
     */
    getActiveModels(): Observable<AIModel[]> {
        this.loadingSubject.next(true)

        return this.apiService.get<AIModel[]>(this.baseUrl, 'AIModel/active').pipe(
            map(models => {
                this.loadingSubject.next(false)
                // The interceptor automatically unwraps successful responses
                const activeModels = Array.isArray(models) ? models : []
                return activeModels
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error fetching active AI models:', error)
                return of([])
            })
        )
    }

    /**
     * GET /api/AIModel/search
     * Search AI models with optional parameters
     */
    searchModels(params?: AIModelSearchParams): Observable<AIModel[]> {
        this.loadingSubject.next(true)

        const queryParams = new URLSearchParams()
        if (params?.name) queryParams.append('name', params.name)
        if (params?.provider) queryParams.append('provider', params.provider)
        if (params?.modelType) queryParams.append('modelType', params.modelType)
        if (params?.isActive !== undefined) queryParams.append('isActive', params.isActive.toString())

        const url = `AIModel/search${queryParams.toString() ? '?' + queryParams.toString() : ''}`

        return this.apiService.get<AIModel[]>(this.baseUrl, url).pipe(
            map(models => {
                this.loadingSubject.next(false)
                if (models) {
                    return Array.isArray(models) ? models : []
                }
                throw new Error('Failed to search AI models')
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error searching AI models:', error)
                throw error
            })
        )
    }

    /**
     * GET /api/AIModel/health
     * Check the health status of the AI model service
     */
    checkHealth(): Observable<boolean> {
        return this.apiService.get<any>(this.baseUrl, 'AIModel/health').pipe(
            map(() => true),
            catchError(error => {
                console.error('AI Model service health check failed:', error)
                return of(false)
            })
        )
    }

    // ========================
    // Utility Methods
    // ========================

    /**
     * Set the current active AI model
     */
    setCurrentModel(model: AIModel | null): void {
        this.currentModelSubject.next(model)
    }

    /**
     * Get the current active AI model synchronously
     */
    getCurrentModel(): AIModel | null {
        return this.currentModelSubject.value
    }

    /**
     * Get current loading state
     */
    isLoading(): boolean {
        return this.loadingSubject.value
    }

    /**
     * Set loading state
     */
    setLoadingState(loading: boolean): void {
        this.loadingSubject.next(loading)
    }

    /**
     * Clear all cached models
     */
    clearCache(): void {
        this.currentModelSubject.next(null)
        this.modelsSubject.next([])
    }

    /**
     * Refresh current model data
     */
    refreshCurrentModel(): Observable<AIModel | null> {
        const currentModel = this.getCurrentModel()
        if (currentModel?.id) {
            return this.getModelById(currentModel.id)
        }
        return new Observable(observer => {
            observer.next(null)
            observer.complete()
        })
    }

    /**
     * Get models by provider
     */
    getModelsByProvider(provider: string): Observable<AIModel[]> {
        return this.searchModels({ provider })
    }

    /**
     * Get models by type
     */
    getModelsByType(modelType: string): Observable<AIModel[]> {
        return this.searchModels({ modelType })
    }

    /**
     * Get model count
     */
    getModelCount(): Observable<number> {
        return this.getAllModels().pipe(
            map(models => models.length)
        )
    }

    /**
     * Get active model count
     */
    getActiveModelCount(): Observable<number> {
        return this.getActiveModels().pipe(
            map(models => models.length)
        )
    }

    /**
     * Check if a model name already exists
     */
    isModelNameExists(name: string): Observable<boolean> {
        return this.searchModels({ name }).pipe(
            map(models => models.some(model =>
                model.name?.toLowerCase() === name.toLowerCase()
            ))
        )
    }

    /**
     * Toggle model active status
     */
    toggleModelStatus(id: number, isActive: boolean): Observable<number> {
        return this.getModelById(id).pipe(
            switchMap(model => {
                const updatedModel: AIModelRequest = {
                    ...model,
                    isActive
                }
                return this.updateModel(id, updatedModel)
            })
        )
    }

    /**
     * Batch delete multiple models
     */
    deleteModels(modelIds: number[], modifiedBy: number = 0): Observable<string[]> {
        const deleteObservables = modelIds.map(id =>
            this.deleteModel(id, modifiedBy)
        )

        return new Observable(observer => {
            Promise.all(deleteObservables.map(obs => obs.toPromise()))
                .then(results => {
                    observer.next(results as string[])
                    observer.complete()
                })
                .catch(error => observer.error(error))
        })
    }

    /**
     * Get the most cost-effective model (lowest cost per token)
     */
    getMostCostEffectiveModel(): Observable<AIModel | null> {
        return this.getActiveModels().pipe(
            map(models => {
                if (models.length === 0) return null

                return models.reduce((cheapest, current) => {
                    const cheapestCost = cheapest.costPerToken || Infinity
                    const currentCost = current.costPerToken || Infinity
                    return currentCost < cheapestCost ? current : cheapest
                })
            })
        )
    }

    /**
     * Get models with highest token capacity
     */
    getHighCapacityModels(minTokens: number = 8000): Observable<AIModel[]> {
        return this.getActiveModels().pipe(
            map(models =>
                models.filter(model => (model.maxTokens || 0) >= minTokens)
            )
        )
    }
}
