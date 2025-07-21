using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BalanceController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Transform beam;           // Le fléau qui va tourner
    [SerializeField] private Transform leftPlate;      // Plateau gauche (pot)
    [SerializeField] private Transform rightPlate;     // Plateau droit (poids fixe)
    [SerializeField] private Transform potCeramic;     // Référence au pot
    
    [Header("Paramètres de la balance")]
    [SerializeField] private float targetWeight = 5f;  // Poids à atteindre
    [SerializeField] private float fixedWeight = 3f;   // Poids du côté droit
    [SerializeField] private float currentPotWeight = 0f; // Poids actuel dans le pot
    [SerializeField] private int itemCount = 0; // Nombre d'objets dans le pot
    [SerializeField] private int itemsRequiredForDoor = 3; // Nombre d'objets pour ouvrir la porte
    
    [Header("Déclencheur par poids")]
    [SerializeField] private bool useWeightTrigger = true; // Utiliser le poids au lieu du nombre d'objets
    [SerializeField] private float triggerWeight = 1.5f; // Poids nécessaire pour déclencher l'événement
    [SerializeField] private bool weightTriggerReached = false; // État du déclencheur
    
    [Header("Animation")]
    [SerializeField] private float maxRotation = 30f;  // Angle max de rotation
    [SerializeField] private float rotationSpeed = 2f; // Vitesse de rotation
    [SerializeField] private float plateauMoveRange = 0.3f; // Déplacement vertical des plateaux
    [SerializeField] private bool invertRotation = false; // Inverser le sens de rotation
    
    [Header("Équilibre")]
    [SerializeField] private float equilibriumThreshold = 0.1f; // Tolérance pour l'équilibre
    [SerializeField] private float stabilityDuration = 2f; // Temps pour valider l'équilibre
    
    [Header("Événements")]
    public UnityEvent onBalanceAchieved;
    public UnityEvent onBalanceLost;
    public UnityEvent onRequiredItemsReached; // Déclenché par nombre d'objets
    public UnityEvent onWeightThresholdReached; // Déclenché par poids
    public UnityEvent onWeightThresholdLost; // Quand on repasse sous le seuil
    
    // Variables privées
    private float currentRotation = 0f;
    private float targetRotation = 0f;
    private bool isBalanced = false;
    private float balanceTimer = 0f;
    private Vector3 leftPlateInitialPos;
    private Vector3 rightPlateInitialPos;
    private float leftPlateDistance;  // Distance du plateau gauche au centre
    private float rightPlateDistance; // Distance du plateau droit au centre
    
    void Start()
    {
        // Sauvegarder les positions initiales EN LOCAL par rapport au beam
        if (leftPlate && beam) 
        {
            Vector3 leftLocalPos = beam.InverseTransformPoint(leftPlate.position);
            leftPlateDistance = leftLocalPos.x; // Distance sur l'axe X local du beam
            leftPlateInitialPos = leftPlate.position;
        }
        
        if (rightPlate && beam) 
        {
            Vector3 rightLocalPos = beam.InverseTransformPoint(rightPlate.position);
            rightPlateDistance = rightLocalPos.x; // Distance sur l'axe X local du beam
            rightPlateInitialPos = rightPlate.position;
        }
        
        // Initialiser la balance
        UpdateBalance();
    }
    
    void Update()
    {
        // Animer la rotation du fléau
        AnimateBeam();
        
        // Animer les plateaux
        AnimatePlates();
        
        // Vérifier l'équilibre
        CheckEquilibrium();
    }
    
    /// <summary>
    /// Ajoute du poids dans le pot
    /// </summary>
    public void AddWeight(float weight)
    {
        currentPotWeight += weight;
        itemCount++;
        UpdateBalance();
        
        // Vérifier le déclencheur par poids
        if (useWeightTrigger)
        {
            CheckWeightTrigger();
        }
        else
        {
            // Vérifier si on a atteint le nombre d'objets requis
            if (itemCount == itemsRequiredForDoor)
            {
                onRequiredItemsReached?.Invoke();
                Debug.Log($"✓ {itemsRequiredForDoor} objets atteints ! Ouverture de la porte...");
            }
        }
    }
    
    /// <summary>
    /// Vérifie si le seuil de poids est atteint
    /// </summary>
    private void CheckWeightTrigger()
    {
        bool wasTriggered = weightTriggerReached;
        weightTriggerReached = currentPotWeight >= triggerWeight;
        
        // Déclencher l'événement si on vient d'atteindre le seuil
        if (!wasTriggered && weightTriggerReached)
        {
            onWeightThresholdReached?.Invoke();
            Debug.Log($"✓ Seuil de poids atteint : {currentPotWeight:F1}kg >= {triggerWeight:F1}kg");
        }
        // Déclencher l'événement si on repasse sous le seuil
        else if (wasTriggered && !weightTriggerReached)
        {
            onWeightThresholdLost?.Invoke();
            Debug.Log($"✗ Seuil de poids perdu : {currentPotWeight:F1}kg < {triggerWeight:F1}kg");
        }
    }
    
    /// <summary>
    /// Définit directement le poids du pot
    /// </summary>
    public void SetPotWeight(float weight)
    {
        currentPotWeight = Mathf.Max(0, weight);
        UpdateBalance();
    }
    
    /// <summary>
    /// Retire du poids du pot
    /// </summary>
    public void RemoveWeight(float weight)
    {
        currentPotWeight = Mathf.Max(0, currentPotWeight - weight);
        itemCount = Mathf.Max(0, itemCount - 1);
        UpdateBalance();
        
        // Vérifier le déclencheur par poids
        if (useWeightTrigger)
        {
            CheckWeightTrigger();
        }
    }
    
    /// <summary>
    /// Obtient l'état du déclencheur de poids
    /// </summary>
    public bool IsWeightTriggerReached()
    {
        return weightTriggerReached;
    }
    
    /// <summary>
    /// Calcule la rotation cible basée sur la différence de poids
    /// </summary>
    private void UpdateBalance()
    {
        // Calculer la différence de poids
        float leftWeight = currentPotWeight;
        float rightWeight = fixedWeight;
        float weightDifference = leftWeight - rightWeight;
        
        // Normaliser la différence (-1 à 1)
        float maxDifference = Mathf.Max(targetWeight, fixedWeight);
        float normalizedDiff = Mathf.Clamp(weightDifference / maxDifference, -1f, 1f);
        
        // Calculer l'angle cible
        // Si leftWeight > rightWeight, le côté gauche descend, donc rotation positive
        // Si rightWeight > leftWeight, le côté droit descend, donc rotation négative
        targetRotation = normalizedDiff * maxRotation;
        
        // Appliquer l'inversion si nécessaire
        if (invertRotation)
            targetRotation = -targetRotation;
        
        Debug.Log($"Poids pot: {currentPotWeight}kg | Poids droit: {fixedWeight}kg | Différence: {weightDifference}kg | Rotation: {targetRotation}°");
    }
    
    /// <summary>
    /// Anime la rotation du fléau
    /// </summary>
    private void AnimateBeam()
    {
        if (!beam) return;
        
        // Interpolation douce vers la rotation cible
        currentRotation = Mathf.Lerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Appliquer la rotation sur l'axe Z
        beam.localRotation = Quaternion.Euler(0, 0, currentRotation);
    }
    
    /// <summary>
    /// Anime le mouvement vertical des plateaux
    /// </summary>
    private void AnimatePlates()
    {
        if (!leftPlate || !rightPlate || !beam) return;

        // Calculer en espace LOCAL par rapport au beam
        float angleRad = currentRotation * Mathf.Deg2Rad;
        
        // Utiliser les distances calculées au Start
        // Position du plateau gauche
        Vector3 leftLocalOffset = new Vector3(
            leftPlateDistance * Mathf.Cos(angleRad), 
            leftPlateDistance * Mathf.Sin(angleRad), 
            0
        );
        
        // Position du plateau droit
        Vector3 rightLocalOffset = new Vector3(
            rightPlateDistance * Mathf.Cos(angleRad), 
            rightPlateDistance * Mathf.Sin(angleRad), 
            0
        );
        
        // Convertir en position monde en utilisant la transformation du beam
        leftPlate.position = beam.TransformPoint(leftLocalOffset);
        rightPlate.position = beam.TransformPoint(rightLocalOffset);
        
        // Garder les plateaux alignés avec le système de balance (pas de rotation)
        leftPlate.rotation = transform.rotation;
        rightPlate.rotation = transform.rotation;
        
        // Si le pot est configuré, le faire suivre le plateau gauche
        if (potCeramic)
        {
            // Position du pot sur le plateau gauche
            Vector3 potPos = leftPlate.position;
            potPos += transform.up * 0.3f; // Utiliser transform.up au lieu de Vector3.up
            potCeramic.position = potPos;
            
            // Garder le pot aligné avec le système
            potCeramic.rotation = transform.rotation;
        }
    }
    
    /// <summary>
    /// Vérifie si la balance est en équilibre
    /// </summary>
    private void CheckEquilibrium()
    {
        // Vérifier si les poids sont équilibrés
        float weightDiff = Mathf.Abs(currentPotWeight - fixedWeight);
        bool inEquilibrium = weightDiff <= equilibriumThreshold;
        
        if (inEquilibrium)
        {
            balanceTimer += Time.deltaTime;
            
            if (balanceTimer >= stabilityDuration && !isBalanced)
            {
                // Équilibre atteint !
                isBalanced = true;
                onBalanceAchieved?.Invoke();
                Debug.Log("✓ Balance équilibrée!");
            }
        }
        else
        {
            balanceTimer = 0f;
            
            if (isBalanced)
            {
                // Équilibre perdu
                isBalanced = false;
                onBalanceLost?.Invoke();
                Debug.Log("✗ Balance déséquilibrée!");
            }
        }
    }
    
    /// <summary>
    /// Obtient le poids nécessaire pour équilibrer
    /// </summary>
    public float GetRequiredWeight()
    {
        return fixedWeight - currentPotWeight;
    }
    
    /// <summary>
    /// Vérifie si la balance est équilibrée
    /// </summary>
    public bool IsBalanced()
    {
        return isBalanced;
    }
    
    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        if (!beam) return;
        
        // Couleur selon l'état
        Gizmos.color = isBalanced ? Color.green : Color.yellow;
        
        // Dessiner la balance
        Vector3 center = beam.position;
        Gizmos.DrawWireSphere(center, 0.1f);
        
        // Dessiner la direction du fléau
        if (leftPlate && rightPlate)
        {
            Gizmos.DrawLine(leftPlate.position, rightPlate.position);
        }
        
        // Afficher les poids
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(leftPlate.position + Vector3.up * 0.5f, $"{currentPotWeight:F1}kg");
            UnityEditor.Handles.Label(rightPlate.position + Vector3.up * 0.5f, $"{fixedWeight:F1}kg");
        }
    }
}