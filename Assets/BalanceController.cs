using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BalanceController : MonoBehaviour
{
    [Header("Références")] [SerializeField]
    private Transform beam; // Le fléau qui va tourner

    [SerializeField] private Transform leftPlate; // Plateau gauche (pot)
    [SerializeField] private Transform rightPlate; // Plateau droit (poids fixe)
    [SerializeField] private Transform potCeramic; // Référence au pot

    [Header("Paramètres de la balance")] [SerializeField]
    private float targetWeight = 5f; // Poids à atteindre

    [SerializeField] private float fixedWeight = 3f; // Poids du côté droit
    [SerializeField] private float currentPotWeight = 0f; // Poids actuel dans le pot

    [Header("Animation")] [SerializeField] private float maxRotation = 30f; // Angle max de rotation
    [SerializeField] private float rotationSpeed = 2f; // Vitesse de rotation
    [SerializeField] private float plateauMoveRange = 0.3f; // Déplacement vertical des plateaux

    [Header("Équilibre")] [SerializeField] private float equilibriumThreshold = 0.1f; // Tolérance pour l'équilibre
    [SerializeField] private float stabilityDuration = 2f; // Temps pour valider l'équilibre

    [Header("Événements")] public UnityEvent onBalanceAchieved;
    public UnityEvent onBalanceLost;

    // Variables privées
    private float currentRotation = 0f;
    private float targetRotation = 0f;
    private bool isBalanced = false;
    private float balanceTimer = 0f;
    private Vector3 leftPlateInitialPos;
    private Vector3 rightPlateInitialPos;

    void Start()
    {
        // Sauvegarder les positions initiales
        if (leftPlate) leftPlateInitialPos = leftPlate.localPosition;
        if (rightPlate) rightPlateInitialPos = rightPlate.localPosition;

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
        UpdateBalance();
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
        UpdateBalance();
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

        // Calculer l'angle cible (positif = penche à droite, négatif = penche à gauche)
        targetRotation = -normalizedDiff * maxRotation;

        Debug.Log($"Poids pot: {currentPotWeight}kg | Différence: {weightDifference}kg | Rotation: {targetRotation}°");
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
        if (!leftPlate || !rightPlate) return;

        // Calculer le déplacement basé sur l'angle
        float rotationPercent = currentRotation / maxRotation;

        // Plateau gauche (inverse du mouvement)
        Vector3 leftPos = leftPlateInitialPos;
        leftPos.y += rotationPercent * plateauMoveRange;
        leftPlate.localPosition = Vector3.Lerp(leftPlate.localPosition, leftPos, rotationSpeed * Time.deltaTime);

        // Plateau droit
        Vector3 rightPos = rightPlateInitialPos;
        rightPos.y -= rotationPercent * plateauMoveRange;
        rightPlate.localPosition = Vector3.Lerp(rightPlate.localPosition, rightPos, rotationSpeed * Time.deltaTime);
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